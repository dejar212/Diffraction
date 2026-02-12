import nbformat as nbf
import os

def update_notebook(path):
    with open(path, 'r', encoding='utf-8') as f:
        nb = nbf.read(f, as_version=4)

    for cell in nb.cells:
        if cell.cell_type == 'code':
            # Update Cell 11 - it had hardcoded matrix_compare.png
            if "Image('cuda_version/plots/matrix_compare.png')" in cell.source:
                cell.source = """# Показывает пути к результатам и графикам
import os
from IPython.display import Image, display

plots_path = 'cuda_version/plots'
if os.path.exists(plots_path):
    print("Доступные графики:", os.listdir(plots_path))
    # Покажем основной график по N
    img_path = os.path.join(plots_path, 'matrix_n_compare.png')
    if os.path.exists(img_path):
        display(Image(img_path))
else:
    print("Папка с графиками не найдена.")"""

            # Ensure Cell 12 is correct
            if "df = pd.read_csv('cuda_version/plots/combined_results.csv')" in cell.source:
                 # Check if it already has the if os.path.exists check
                 if "if os.path.exists(results_path):" not in cell.source:
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
    print("Файл результатов не найден.")"""

    with open(path, 'w', encoding='utf-8') as f:
        nbf.write(nb, f)

update_notebook('diff-cuda-c++.ipynb')

