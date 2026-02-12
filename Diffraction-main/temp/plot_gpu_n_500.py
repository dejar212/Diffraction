import matplotlib.pyplot as plt
import numpy as np
import csv
import os

def generate_from_real_data(file_path):
    N = []
    matrix_times = []
    solve_times = []
    total_times = []
    
    if not os.path.exists(file_path) or os.path.getsize(file_path) == 0:
        print(f"Файл {file_path} пуст или не найден! Бенчмарк завершился с ошибкой.")
        return
    
    with open(file_path, 'r') as f:
        lines = f.readlines()
        for line in lines:
            parts = line.split()
            # Пропускаем заголовки и пустые строки, ищем строки начинающиеся с числа (N)
            if parts and parts[0].isdigit():
                N.append(int(parts[0]))
                matrix_times.append(float(parts[1]))
                solve_times.append(float(parts[2]))
                total_times.append(float(parts[3]))

    N = np.array(N)
    total_times = np.array(total_times)

    # Построение графика
    plt.figure(figsize=(10, 7), dpi=100)
    plt.grid(True, linestyle='--', alpha=0.5)
    
    # Реальные данные
    plt.plot(N, total_times, marker='o', markerfacecolor='white', markeredgecolor='#348ABD', 
             linestyle='-', color='#348ABD', label='Построение матрицы на GPU (реальные данные)', 
             linewidth=1.5, markersize=5)

    # Аппроксимация параболой
    coeffs = np.polyfit(N, total_times, 2)
    p = np.poly1d(coeffs)
    plt.plot(N, p(N), linestyle='--', color='#E24A33', 
             label=r'Аппроксимация $T(N) \approx a \cdot N^2 + b \cdot N + c$', 
             linewidth=2)

    # Метки для первых 5 точек и последней
    offsets = [(0, 12), (0, -18), (0, 12), (0, -18), (0, 12)]
    for i in range(min(len(N), 5)):
        plt.annotate(f'{total_times[i]:.1f}', (N[i], total_times[i]), textcoords="offset points", 
                     xytext=offsets[i], ha='center', fontsize=8, color='grey',
                     bbox=dict(boxstyle="round,pad=0.2", fc="white", ec="none", alpha=0.7))
    
    if len(N) > 5:
        plt.annotate(f'{total_times[-1]:.1f}', (N[-1], total_times[-1]), textcoords="offset points", 
                     xytext=(0,10), ha='center', fontsize=8, color='grey')

    plt.title('Время построения матрицы и параболическая аппроксимация', fontsize=14, fontweight='bold', pad=20)
    plt.xlabel('Параметр усечения N', fontsize=12)
    plt.ylabel('Время (мс)', fontsize=12)
    plt.legend(loc='upper left', frameon=True, fontsize=10)
    plt.xlim(0, max(N) + 20)
    plt.ylim(0, max(total_times) * 1.2)
    
    plt.tight_layout()
    plt.savefig('temp/gpu_performance_n_500.png')
    
    # Сохранение CSV
    with open('temp/gpu_performance_data.csv', 'w', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(['index', 'N', 'MatrixGPU_ms_CUDA', 'SolveTime_ms_CUDA', 'Total_ms_CUDA'])
        for i in range(len(N)):
            writer.writerow([i, N[i], f"{matrix_times[i]:.4f}", f"{solve_times[i]:.4f}", f"{total_times[i]:.4f}"])

    print("График обновлен: temp/gpu_performance_n_500.png")
    print("CSV обновлен реальными данными: temp/gpu_performance_data.csv")

# Пытаемся прочитать реальные данные из файла, если он есть
results_path = 'temp/raw_results.txt'
generate_from_real_data(results_path)
plt.show()

