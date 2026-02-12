import nbformat as nbf
import os

def update_notebook(path):
    with open(path, 'r', encoding='utf-8') as f:
        nb = nbf.read(f, as_version=4)

    for cell in nb.cells:
        if cell.cell_type == 'code':
            # Update Cell 12
            if "df = pd.read_csv('cuda_version/plots/combined_results.csv')" in cell.source:
                cell.source = """import pandas as pd
import matplotlib.pyplot as plt
import os

results_path = 'cuda_version/plots/combined_results.csv'
if os.path.exists(results_path):
    df = pd.read_csv(results_path)
    
    # 1. Сравнение СЛАУ
    plt.figure(figsize=(10, 5))
    plt.plot(df['N'], df['SolveTime_ms_CPU'], 'o-', label='СЛАУ CPU (Gauss)')
    plt.plot(df['N'], df['SolveTime_ms_CUDA'], 's-', label='СЛАУ CUDA (cuSOLVER)')
    plt.xlabel("N")
    plt.ylabel("Время (мс)")
    plt.title("Время решения СЛАУ")
    plt.legend()
    plt.grid(alpha=0.3)
    plt.show()

    # 2. Сравнение матрицы (Зависимость от N)
    plt.figure(figsize=(10, 5))
    plt.plot(df['N'], df['MatrixTime_ms_CPU'], 'o-', label='Матрица CPU')
    plt.plot(df['N'], df['MatrixGPU_ms_CUDA'], 's-', label='Матрица GPU')
    plt.xlabel("N")
    plt.ylabel("Время (мс)")
    plt.title("Построение матрицы (при фиксированном M=20)")
    plt.yscale('log')
    plt.legend()
    plt.grid(alpha=0.3)
    plt.show()
else:
    print("Файл результатов не найден. Сначала запустите бенчмарк.")"""

            # Update Cell 13
            elif "speedup_matrix = df['MatrixTime_ms_CPU'] / df['MatrixGPU_ms_CUDA']" in cell.source and "speedup_solve =" in cell.source:
                 cell.source = """# Графики ускорения (Speedup)
if os.path.exists(results_path):
    plt.figure(figsize=(10, 5))
    speedup_matrix = df['MatrixTime_ms_CPU'] / df['MatrixGPU_ms_CUDA']
    plt.plot(df['N'], speedup_matrix, 'bs-', linewidth=2, label='Ускорение построения матрицы')
    plt.xlabel("N")
    plt.ylabel("Ускорение (раз)")
    plt.title("Эффективность GPU ускорения в зависимости от N")
    plt.legend()
    plt.grid(True, alpha=0.3)
    plt.show()"""

            # Update Cell 14
            elif "gpu_perf = 'cuda_version/plots/gpu_performance.png'" in cell.source:
                cell.source = """# Дополнительно: Зависимость от M
import glob
from IPython.display import Image, display

m_plot = 'cuda_version/plots/matrix_m_compare.png'
if os.path.exists(m_plot):
    print("Зависимость от числа узлов квадратуры M:")
    display(Image(m_plot))
    print("Ускорение в зависимости от M:")
    display(Image('cuda_version/plots/speedup_m.png'))
else:
    print("Результаты для M не найдены.")"""

    with open(path, 'w', encoding='utf-8') as f:
        nbf.write(nb, f)

update_notebook('diff-cuda-c++.ipynb')

