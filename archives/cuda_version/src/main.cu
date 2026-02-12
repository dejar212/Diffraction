#include "DifrOnLentaCuda.h"
#include <iostream>
#include <iomanip>
#include <vector>
#include <cmath>

#ifdef _WIN32
#include <windows.h>
#endif

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

void PrintCoefficients(const DifrOnLentaCuda& solver) {
    std::cout << "\nКоэффициенты Чебышева (" << solver.N << " шт):\n";
    std::cout << std::left << std::setw(6) << "#"
              << std::setw(30) << "Значение" << "\n";

    for (int i = 0; i < solver.N; i++) {
        const Complex& value = solver.y[i];
        std::cout << std::left << std::setw(6) << (i + 1)
                  << std::fixed << std::setprecision(4)
                  << value.real() << " + "
                  << value.imag() << "i\n";
    }
}

bool SolveAndPrint(double a, double b, double lambda, double theta, int N, double skinDepth, bool verbose = true) {
    DifrOnLentaCuda solver(a, b, lambda, theta, N, skinDepth);
    int result = solver.SolveDifr();
    if (result != 1) {
        std::cerr << "Ошибка решения СЛАУ!\n";
        return false;
    }

    if (verbose) {
        std::cout << "\nПараметры:\n";
        std::cout << "a = " << a << ", b = " << b << "\n";
        std::cout << "λ = " << lambda << ", θ = " << (theta * 180.0 / M_PI) << "°\n";
        std::cout << "N = " << N << ", δ = " << skinDepth << "\n";

        std::cout << "\nИмпедансный коэффициент χ: "
                  << std::fixed << std::setprecision(6)
                  << solver.chi.real() << " + "
                  << solver.chi.imag() << "i\n";

        PrintCoefficients(solver);

        std::cout << "\nВремя выполнения:\n";
        std::cout << std::fixed << std::setprecision(3);
        std::cout << "Построение матрицы (GPU): " << solver.lastTiming.matrixBuildTime << " мс\n";
        std::cout << "Решение СЛАУ (GPU cuSOLVER): " << solver.lastTiming.solveTime << " мс\n";
        std::cout << "Общее: " << solver.lastTiming.totalTime << " мс\n";
    }

    return true;
}

int main() {
#ifdef _WIN32
    SetConsoleOutputCP(CP_UTF8);
#endif

    std::cout << "\nCUDA решатель дифракции на ленте\n";

    const double a = -1.0;
    const double b = 1.0;
    const double lambda = 1.0;
    const double theta = M_PI / 4.0;
    const int N_default = 10;
    const double skinDepth = 0.1;

    std::cout << "\n--- Случай 1: Идеальный проводник (без скин-эффекта) ---\n";
    SolveAndPrint(a, b, lambda, theta, N_default, 0.0, true);

    std::cout << "\n--- Случай 2: С учетом скин-эффекта (delta = " << skinDepth << ") ---\n";
    SolveAndPrint(a, b, lambda, theta, N_default, skinDepth, true);

    std::cout << "\n\nТестирование зависимости от N\n";
    // Набор N от 10 до 50 с шагом 10
    std::vector<int> N_values = {10, 20, 30, 40, 50};

    std::cout << "\n" << std::left
              << std::setw(8) << "N"
              << std::setw(25) << "GPU матрица (мс)"
              << std::setw(25) << "GPU СЛАУ (мс)"
              << std::setw(18) << "Общее (мс)" << "\n";

    for (int N : N_values) {
        DifrOnLentaCuda solver(a, b, lambda, theta, N, skinDepth);
        int result = solver.SolveDifr();
        if (result == 1) {
            std::cout << std::left << std::setw(8) << N
                      << std::fixed << std::setprecision(3)
                      << std::setw(25) << solver.lastTiming.matrixBuildTime
                      << std::setw(25) << solver.lastTiming.solveTime
                      << std::setw(18) << solver.lastTiming.totalTime << "\n";
        } else {
            std::cout << std::left << std::setw(8) << N << "Ошибка решения!\n";
        }
    }

    std::cout << "\n\nДанные для построения графиков (CSV):\n";
    std::cout << "N,MatrixGPU_ms,SolveCPU_ms,Total_ms\n";

    for (int N : N_values) {
        DifrOnLentaCuda solver(a, b, lambda, theta, N, skinDepth);
        if (solver.SolveDifr() == 1) {
            std::cout << N << ","
                      << std::fixed << std::setprecision(6)
                      << solver.lastTiming.matrixBuildTime << ","
                      << solver.lastTiming.solveTime << ","
                      << solver.lastTiming.totalTime << "\n";
        }
    }

    // 3. Тестирование зависимости от M
    std::cout << "\n\nТестирование зависимости от M (при фиксированном N = " << N_default << ")\n";
    std::vector<int> M_values = {10, 20, 50, 100, 200};
    std::cout << "M,MatrixGPU_ms,SolveCPU_ms,Total_ms\n";
    for (int M : M_values) {
        DifrOnLentaCuda solver(a, b, lambda, theta, N_default, skinDepth, M);
        if (solver.SolveDifr() == 1) {
            std::cout << M << ","
                      << std::fixed << std::setprecision(6)
                      << solver.lastTiming.matrixBuildTime << ","
                      << solver.lastTiming.solveTime << ","
                      << solver.lastTiming.totalTime << "\n";
        }
    }

    // 4. Тестирование 3D (N и M одновременно)
    std::cout << "\n\nТестирование 3D (N и M)\n";
    std::vector<int> N_3d = {10, 20, 50, 100, 200};
    std::vector<int> M_3d = {10, 20, 50, 100, 200};
    std::cout << "N,M,MatrixGPU_ms\n";
    for (int n : N_3d) {
        for (int m : M_3d) {
            DifrOnLentaCuda solver(a, b, lambda, theta, n, skinDepth, m);
            if (solver.SolveDifr() == 1) {
                std::cout << n << "," << m << "," << solver.lastTiming.matrixBuildTime << "\n";
            }
        }
    }

    return 0;
}
