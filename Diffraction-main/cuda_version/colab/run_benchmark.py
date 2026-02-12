#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Colab helper script that
1) Installs build dependencies (cmake, build-essential, ninja, python libs)
2) Builds CPU (cpp_version) and CUDA (cuda_version) solvers via CMake
3) Runs executables, captures timing CSV blocks
4) Plots CPU vs CUDA timings and speedups

Usage inside Google Colab:
    !python cuda_version/colab/run_benchmark.py
"""

from __future__ import annotations

import json
import re
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Tuple

import matplotlib.pyplot as plt

ROOT = Path(__file__).resolve().parents[1]
CPU_SRC = ROOT.parent / "cpp_version"
CUDA_SRC = ROOT
CPU_BUILD = CPU_SRC / "build_colab"
CUDA_BUILD = CUDA_SRC / "build"
PLOTS_DIR = ROOT / "plots"
PLOTS_DIR.mkdir(parents=True, exist_ok=True)


def run_cmd(cmd: List[str], cwd: Path | None = None, env: Dict[str, str] | None = None) -> str:
    print(f"\n[cmd] {' '.join(cmd)}")
    process = subprocess.run(
        cmd,
        cwd=cwd,
        env=env,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        check=True,
        text=True,
    )
    print(process.stdout)
    return process.stdout


def ensure_packages() -> None:
    print("Проверка наличия CMake/compilers...")
    try:
        run_cmd(["cmake", "--version"])
    except (OSError, subprocess.CalledProcessError):
        run_cmd(["sudo", "apt-get", "update"])
        run_cmd(
            ["sudo", "apt-get", "install", "-y", "cmake", "build-essential", "ninja-build"]
        )

    print("Установка python-библиотек для графиков...")
    run_cmd([sys.executable, "-m", "pip", "install", "--quiet", "matplotlib", "pandas", "numpy"])


def ensure_gpu() -> None:
    try:
        run_cmd(["nvidia-smi"])
    except Exception as exc:  # pylint: disable=broad-except
        raise RuntimeError(
            "GPU недоступен. В Colab включите GPU (Runtime → Change runtime type → GPU)."
        ) from exc


def get_frequencies() -> Tuple[str, str]:
    cpu_freq = "Unknown"
    gpu_freq = "Unknown"
    
    try:
        # Get CPU info
        cpu_info = subprocess.check_output("lscpu", shell=True, text=True)
        for line in cpu_info.splitlines():
            if "Model name" in line:
                cpu_freq = line.split(":")[1].strip()
            if "CPU max MHz" in line:
                mhz = float(line.split(":")[1].strip())
                cpu_freq += f" @ {mhz/1000.0:.2f} GHz"
    except Exception:
        try:
            # Fallback for some environments
            cpu_info = subprocess.check_output("grep 'model name' /proc/cpuinfo | head -n 1", shell=True, text=True)
            cpu_freq = cpu_info.split(":")[1].strip()
        except Exception:
            pass

    try:
        # Get GPU info
        gpu_info = subprocess.check_output("nvidia-smi --query-gpu=name,clocks.max.sm --format=csv,noheader", shell=True, text=True)
        gpu_freq = gpu_info.strip()
    except Exception:
        pass
        
    return cpu_freq, gpu_freq


def configure_and_build(src_dir: Path, build_dir: Path, cmake_args: List[str]) -> None:
    build_dir.mkdir(parents=True, exist_ok=True)
    run_cmd(
        ["cmake", "-S", str(src_dir), "-B", str(build_dir), "-G", "Ninja", *cmake_args]
    )
    run_cmd(["cmake", "--build", str(build_dir), "--config", "Release"])


def parse_csv_block(text: str, marker: str = "N,") -> List[Tuple[int, float, float, float]]:
    csv_pattern = re.compile(rf"^\s*{marker}\w+", re.MULTILINE)
    header_match = csv_pattern.search(text)
    if not header_match:
        return []

    start = header_match.start()
    # Find next blank line or end of text to isolate CSV block
    end = text.find("\n\n", start)
    if end == -1:
        end = len(text)
    
    lines = text[start:end].strip().splitlines()
    if not lines:
        return []
        
    header = lines[0].strip().split(",")
    rows: List[Tuple[int, float, float, float]] = []
    for line in lines[1:]:
        if not line.strip():
            continue
        parts = line.split(",")
        if len(parts) != len(header):
            continue
        try:
            val0 = int(parts[0])
            values = tuple(float(p) for p in parts[1:])
            rows.append((val0, *values))
        except ValueError:
            continue
    return rows


@dataclass
class TimingDataset:
    label: str
    rows: List[Tuple[int, float, float, float]]
    header: Tuple[str, str, str, str]

    def to_dict(self) -> Dict[str, List[float]]:
        if not self.rows: return {}
        data: Dict[str, List[float]] = {self.header[0]: []}
        for h in self.header[1:]:
            data[h] = []
        for row in self.rows:
            data[self.header[0]].append(row[0])
            for idx, h in enumerate(self.header[1:], start=1):
                data[h].append(row[idx])
        return data


def run_solver(executable: Path) -> str:
    return run_cmd([str(executable)])


def datasets_from_output(text: str, label: str) -> Tuple[TimingDataset, TimingDataset, List[Tuple[int, int, float]]]:
    # Extract N-dependence
    n_rows = parse_csv_block(text, "N,")
    n_header = ("N", "MatrixTime_ms", "SolveTime_ms", "TotalTime_ms") if label == "CPU" else \
               ("N", "MatrixGPU_ms", "SolveCPU_ms", "Total_ms")
    
    # Extract M-dependence
    m_rows = parse_csv_block(text, "M,")
    m_header = ("M", "MatrixTime_ms", "SolveTime_ms", "TotalTime_ms") if label == "CPU" else \
               ("M", "MatrixGPU_ms", "SolveCPU_ms", "Total_ms")
               
    # Extract 3D data
    rows_3d = []
    marker_3d = "N,M,"
    csv_pattern = re.compile(rf"^\s*{marker_3d}\w+", re.MULTILINE)
    match = csv_pattern.search(text)
    if match:
        start = match.start()
        end = text.find("\n\n", start)
        if end == -1: end = len(text)
        lines = text[start:end].strip().splitlines()
        for line in lines[1:]:
            parts = line.split(",")
            if len(parts) == 3:
                try:
                    rows_3d.append((int(parts[0]), int(parts[1]), float(parts[2])))
                except ValueError: continue

    return TimingDataset(label=label, rows=n_rows, header=n_header), \
           TimingDataset(label=label, rows=m_rows, header=m_header), \
           rows_3d


def plot_comparison(cpu_n: TimingDataset, cuda_n: TimingDataset, cpu_m: TimingDataset, cuda_m: TimingDataset, cpu_3d, cuda_3d) -> None:
    import pandas as pd
    import numpy as np
    from mpl_toolkits.mplot3d import Axes3D

    # ... existing plot logic (N, M, Speedup) ...
    if cpu_n.rows and cuda_n.rows:
        cpu_df = pd.DataFrame(cpu_n.rows, columns=cpu_n.header).rename(
            columns={
                cpu_n.header[1]: "MatrixTime_ms_CPU",
                cpu_n.header[2]: "SolveTime_ms_CPU",
                cpu_n.header[3]: "TotalTime_ms_CPU",
            }
        )
        cuda_df = pd.DataFrame(cuda_n.rows, columns=cuda_n.header).rename(
            columns={
                cuda_n.header[1]: "MatrixGPU_ms_CUDA",
                cuda_n.header[2]: "SolveTime_ms_CUDA",
                cuda_n.header[3]: "Total_ms_CUDA",
            }
        )
        merged = cpu_df.merge(cuda_df, on="N")

        plt.style.use("seaborn-v0_8")
        fig, ax = plt.subplots(figsize=(10, 6))
        ax.plot(merged["N"], merged["MatrixTime_ms_CPU"], "o-", label="Матрица CPU", linewidth=2)
        ax.plot(merged["N"], merged["MatrixGPU_ms_CUDA"], "s-", label="Матрица GPU", linewidth=2)
        ax.set_xlabel("N")
        ax.set_ylabel("Время (мс)")
        ax.set_title("Зависимость от N (при фиксированном M=20)")
        ax.grid(True, alpha=0.3)
        ax.legend()
        fig.tight_layout()
        fig.savefig(PLOTS_DIR / "matrix_n_compare.png", dpi=300)

        # Speedup plot for N
        fig2, ax2 = plt.subplots(figsize=(10, 6))
        ax2.plot(merged["N"], merged["MatrixTime_ms_CPU"] / merged["MatrixGPU_ms_CUDA"], 'o-', label="Ускорение матрицы", linewidth=2)
        ax2.set_xlabel("N")
        ax2.set_ylabel("Ускорение (раз)")
        ax2.set_title("Ускорение GPU (по N)")
        ax2.grid(True, alpha=0.3)
        ax2.legend()
        fig2.tight_layout()
        fig2.savefig(PLOTS_DIR / "speedup_n.png", dpi=300)
        
        # Save results to CSV for notebook
        merged.to_csv(PLOTS_DIR / "combined_results.csv", index=False)

    # 2. Plot M-dependence
    if cpu_m.rows and cuda_m.rows:
        cpu_df_m = pd.DataFrame(cpu_m.rows, columns=cpu_m.header).rename(
            columns={
                cpu_m.header[1]: "MatrixTime_ms_CPU",
                cpu_m.header[2]: "SolveTime_ms_CPU",
                cpu_m.header[3]: "TotalTime_ms_CPU",
            }
        )
        cuda_df_m = pd.DataFrame(cuda_m.rows, columns=cuda_m.header).rename(
            columns={
                cuda_m.header[1]: "MatrixGPU_ms_CUDA",
                cuda_m.header[2]: "SolveTime_ms_CUDA",
                cuda_m.header[3]: "Total_ms_CUDA",
            }
        )
        merged_m = cpu_df_m.merge(cuda_df_m, on="M")

        fig3, ax3 = plt.subplots(figsize=(10, 6))
        ax3.plot(merged_m["M"], merged_m["MatrixTime_ms_CPU"], "o-", label="Матрица CPU", linewidth=2)
        ax3.plot(merged_m["M"], merged_m["MatrixGPU_ms_CUDA"], "s-", label="Матрица GPU", linewidth=2)
        ax3.set_xlabel("M (узлы квадратуры)")
        ax3.set_ylabel("Время (мс)")
        ax3.set_title("Зависимость от M (при фиксированном N=10)")
        ax3.grid(True, alpha=0.3)
        ax3.legend()
        fig3.tight_layout()
        fig3.savefig(PLOTS_DIR / "matrix_m_compare.png", dpi=300)

        # Speedup plot for M
        fig4, ax4 = plt.subplots(figsize=(10, 6))
        ax4.plot(merged_m["M"], merged_m["MatrixTime_ms_CPU"] / merged_m["MatrixGPU_ms_CUDA"], 's-', label="Ускорение матрицы", color='green', linewidth=2)
        ax4.set_xlabel("M")
        ax4.set_ylabel("Ускорение (раз)")
        ax4.set_title("Ускорение GPU (по M)")
        ax4.grid(True, alpha=0.3)
        ax4.legend()
        fig4.tight_layout()
        fig4.savefig(PLOTS_DIR / "speedup_m.png", dpi=300)
        
        # Save M results
        merged_m.to_csv(PLOTS_DIR / "combined_results_m.csv", index=False)

    # 3. Plot 3D surfaces
    for label, data_3d in [("CPU", cpu_3d), ("CUDA", cuda_3d)]:
        if not data_3d: continue
        df3 = pd.DataFrame(data_3d, columns=["N", "M", "Time"])
        
        fig3d = plt.figure(figsize=(12, 8))
        ax3d = fig3d.add_subplot(111, projection='3d')
        
        # Pivot for surface plot
        pivot = df3.pivot(index='N', columns='M', values='Time')
        X, Y = np.meshgrid(pivot.columns, pivot.index)
        Z = pivot.values
        
        surf = ax3d.plot_surface(X, Y, Z, cmap='viridis', edgecolor='none', alpha=0.8)
        
        ax3d.set_xlabel('M (узлы квадратуры)')
        ax3d.set_ylabel('N (базисные функции)')
        ax3d.set_zlabel('Время (мс)')
        ax3d.set_title(f'Поверхность сложности: {label}')
        
        fig3d.colorbar(surf, ax=ax3d, shrink=0.5, aspect=5)
        fig3d.tight_layout()
        fig3d.savefig(PLOTS_DIR / f"surface_3d_{label.lower()}.png", dpi=300)

    print(f"Результаты и графики сохранены в {PLOTS_DIR}")


def main() -> None:
    ensure_packages()
    ensure_gpu()

    cpu_freq, gpu_freq = get_frequencies()
    print(f"\nСреда выполнения:")
    print(f"CPU: {cpu_freq}")
    print(f"GPU: {gpu_freq}\n")

    # Explicit frequency-aware comparison
    try:
        cpu_f = float(re.search(r"(\d+\.\d+)\s*GHz", cpu_freq).group(1)) if "GHz" in cpu_freq else 2.2
        gpu_f = float(re.search(r"(\d+)\s*MHz", gpu_freq).group(1)) / 1000.0 if "MHz" in gpu_freq else 1.59
        print(f"Анализ частот: CPU {cpu_f:.2f} GHz vs GPU {gpu_f:.2f} GHz")
    except Exception:
        cpu_f, gpu_f = 2.2, 1.59

    configure_and_build(CPU_SRC, CPU_BUILD, ["-DCMAKE_BUILD_TYPE=Release"])
    cpu_output = run_solver(CPU_BUILD / "diffraction_solver")
    cpu_n, cpu_m, cpu_3d = datasets_from_output(cpu_output, "CPU")

    configure_and_build(
        CUDA_SRC,
        CUDA_BUILD,
        ["-DCMAKE_BUILD_TYPE=Release", "-DCMAKE_CUDA_ARCHITECTURES=75"],
    )
    cuda_output = run_solver(CUDA_BUILD / "CudaDiffr")
    cuda_n, cuda_m, cuda_3d = datasets_from_output(cuda_output, "CUDA")

    plot_comparison(cpu_n, cuda_n, cpu_m, cuda_m, cpu_3d, cuda_3d)


if __name__ == "__main__":
    main()
