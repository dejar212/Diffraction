#include "DifrOnLentaCuda.h"
#include <iostream>
#include <iomanip>
#include <vector>
#include <cmath>

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

int main() {
    std::cout << "GPU Benchmark for N = 10 to 50 (Step 10)\n";
    std::cout << "------------------------------------------\n";

    const double a = -1.0;
    const double b = 1.0;
    const double lambda = 1.0;
    const double theta = M_PI / 4.0;
    const double skinDepth = 0.1;
    const int M = 20;

    std::cout << std::left
              << std::setw(8) << "N"
              << std::setw(20) << "Matrix GPU (ms)"
              << std::setw(20) << "Solve GPU (ms)"
              << std::setw(15) << "Total (ms)" << "\n";

    std::vector<int> N_values;
    for (int n = 10; n <= 500; n += 10) {
        N_values.push_back(n);
    }

    for (int N : N_values) {
        DifrOnLentaCuda solver(a, b, lambda, theta, N, skinDepth, M);
        if (solver.SolveDifr() == 1) {
            std::cout << std::left << std::setw(8) << N
                      << std::fixed << std::setprecision(3)
                      << std::setw(20) << solver.lastTiming.matrixBuildTime
                      << std::setw(20) << solver.lastTiming.solveTime
                      << std::setw(15) << solver.lastTiming.totalTime << "\n";
        } else {
            std::cout << std::left << std::setw(8) << N << "Error!\n";
        }
    }

    return 0;
}

