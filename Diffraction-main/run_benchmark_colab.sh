#!/bin/bash

# Скрипт для автоматического запуска бенчмарка в Google Colab
# Генерирует реальные данные (CSV и график) на основе текущего GPU

echo "=== Шаг 1: Обновление репозитория ==="
git pull origin main

echo "=== Шаг 2: Создание папок ==="
mkdir -p temp
mkdir -p cuda_version/build

echo "=== Шаг 3: Сборка бенчмарка (GpuBenchmarkN) ==="
cd cuda_version
rm -rf build
mkdir -p build
cd build
# Принудительно задаем архитектуру 75 (Tesla T4) и отключаем PTX-конфликты
cmake .. -DCMAKE_CUDA_ARCHITECTURES=75 -DCMAKE_BUILD_TYPE=Release
make GpuBenchmarkN

if [ $? -eq 0 ]; then
    echo "=== Шаг 4: Запуск реального бенчмарка (N=10..500) ==="
    echo "Это может занять пару минут, пожалуйста, подождите..."
    ./GpuBenchmarkN > ../../temp/raw_results.txt
    
    echo "=== Шаг 5: Генерация CSV и графика ==="
    cd ../..
    python3 temp/plot_gpu_n_500.py
    
    echo ""
    echo "Все готово!"
    echo "Реальные данные сохранены в: temp/gpu_performance_data.csv"
    echo "График сохранен в: temp/gpu_performance_n_500.png"
else
    echo "Ошибка при сборке! Проверьте, выбран ли тип среды с GPU в Colab."
fi

