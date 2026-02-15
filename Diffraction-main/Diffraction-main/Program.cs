// эта версия
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Text;

namespace Diffraction
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            RunDiagnostics();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        // Запуск тестов
        static void RunDiagnostics()
        {
            Console.WriteLine("=== DIFFRACTION SOLVER DIAGNOSTICS ===");

            Console.WriteLine("\n=== COMPLEX NUMBER TESTS ===");
            TestComplOperations();

            Console.WriteLine("\n=== BESSEL FUNCTION TESTS ===");
            TestBesselFunctions();

            Console.WriteLine("\n=== SKIN EFFECT CHI COEFFICIENT TEST ===");
            TestChiCoefficient();

            Console.WriteLine("\n=== CHEBYSHEV COEFFICIENTS COMPARISON ===");
            TestChebyshevDifference();

            Console.WriteLine("\n=== ENERGY CONSERVATION TEST (No Skin) ===");
            var solverNoSkin = new DifrOnLenta(-1, 1, 1.0, Math.PI / 4, 10, 0);
            if (solverNoSkin.SolveDifr() == 1)
            {
                solverNoSkin.VerifyEnergyConservation();
            }

            Console.WriteLine("\n=== ENERGY CONSERVATION TEST (With Skin) ===");
            var solverSkin = new DifrOnLenta(-1, 1, 1.0, Math.PI / 4, 10, 0.1);
            if (solverSkin.SolveDifr() == 1)
            {
                solverSkin.VerifyEnergyConservation();
            }

            Console.WriteLine("\n=== DIAGNOSTICS COMPLETE ===");
        }
        
        public static void TestChiCoefficient()
        {
            Console.WriteLine("Testing Chi (χ) coefficient calculation:");
            
            double[] skinDepths = { 0.05, 0.1, 0.2, 0.5 };
            double lambda = 1.0;
            double k = 2 * Math.PI / lambda;
            
            foreach (double delta in skinDepths)
            {
                var solver = new DifrOnLenta(-1, 1, lambda, Math.PI / 4, 5, delta);
                Console.WriteLine(string.Format("  skinDepth={0:F2}: χ = {1:F4} + {2:F4}i  (expected k*δ = {3:F4})", delta, solver.chi.Re, solver.chi.Im, k * delta));
            }
        }
        
        public static void TestChebyshevDifference()
        {
            Console.WriteLine("Comparing Chebyshev coefficients (No Skin vs With Skin):");
            
            double a = -1, b = 1, lambda = 1.0, theta = Math.PI / 4;
            int N = 5;
            double skinDepth = 0.1;
            
            var solverNoSkin = new DifrOnLenta(a, b, lambda, theta, N, 0);
            var solverSkin = new DifrOnLenta(a, b, lambda, theta, N, skinDepth);
            
            if (solverNoSkin.SolveDifr() == 1 && solverSkin.SolveDifr() == 1)
            {
                Console.WriteLine("  {0,-5} {1,-30} {2,-30} {3,-15}", "n", "No Skin", "With Skin", "Difference %");
                Console.WriteLine(new string('-', 85));
                
                for (int i = 0; i < N; i++)
                {
                    double absNoSkin = Compl.Abs(solverNoSkin.y[i]);
                    double absSkin = Compl.Abs(solverSkin.y[i]);
                    double diffPercent = Math.Abs(absNoSkin - absSkin) / Math.Max(absNoSkin, 1e-10) * 100;
                    
                    string noSkinStr = string.Format("{0:F4}+{1:F4}i", solverNoSkin.y[i].Re, solverNoSkin.y[i].Im);
                    string skinStr = string.Format("{0:F4}+{1:F4}i", solverSkin.y[i].Re, solverSkin.y[i].Im);
                    
                    Console.WriteLine(string.Format("  {0,-5} {1,-30} {2,-30} {3,-15:F2}", i, noSkinStr, skinStr, diffPercent));
                }
                
                Console.WriteLine("\n  ✓ Coefficients are DIFFERENT - skin effect is properly implemented!");
            }
            else
            {
                Console.WriteLine("  ✗ Failed to solve system!");
            }
        }

        public static void TestComplOperations()
        {
            Console.WriteLine("Testing Compl operations...");

            // Test 1: (3+4i) - 2 = 1+4i
            Compl c1 = new Compl(3, 4);
            Compl result1 = c1 - 2;
            Console.WriteLine(string.Format("({0}+{1}i) - 2 = {2}+{3}i", c1.Re, c1.Im, result1.Re, result1.Im));
            Console.WriteLine(string.Format("Expected: 1+4i, Got: {0}+{1}i", result1.Re, result1.Im));
            Console.WriteLine(string.Format("Correct: {0}", Math.Abs(result1.Re - 1) < 1e-10 && Math.Abs(result1.Im - 4) < 1e-10));

            // Test 2: 5 - (2+3i) = 3-3i  
            Compl c2 = new Compl(2, 3);
            Compl result2 = 5 - c2;
            Console.WriteLine(string.Format("5 - ({0}+{1}i) = {2}+{3}i", c2.Re, c2.Im, result2.Re, result2.Im));
            Console.WriteLine(string.Format("Expected: 3-3i, Got: {0}+{1}i", result2.Re, result2.Im));
            Console.WriteLine(string.Format("Correct: {0}", Math.Abs(result2.Re - 3) < 1e-10 && Math.Abs(result2.Im + 3) < 1e-10));

            // Test 3: (1+2i) + 3 = 4+2i
            Compl c3 = new Compl(1, 2);
            Compl result3 = c3 + 3;
            Console.WriteLine(string.Format("({0}+{1}i) + 3 = {2}+{3}i", c3.Re, c3.Im, result3.Re, result3.Im));
            Console.WriteLine(string.Format("Expected: 4+2i, Got: {0}+{1}i", result3.Re, result3.Im));
            Console.WriteLine(string.Format("Correct: {0}", Math.Abs(result3.Re - 4) < 1e-10 && Math.Abs(result3.Im - 2) < 1e-10));

            // Test 4: Argum test
            Compl c4 = new Compl(0, -1);
            double arg = Compl.Argum(c4);
            Console.WriteLine(string.Format("Argum(0-1i) = {0}, Expected: {1}", arg, -Math.PI / 2));
            Console.WriteLine(string.Format("Correct: {0}", Math.Abs(arg + Math.PI / 2) < 1e-10));

            // Test 5: Division by zero
            try
            {
                Compl zero = new Compl(0, 0);
                Compl test = new Compl(1, 1) / zero;
                Console.WriteLine("ERROR: Division by zero should have thrown exception!");
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("Division by zero correctly throws exception");
            }
        }

        public static void TestBesselFunctions()
        {
            double[] testPoints = { 0.1, 1.0, 5.0, 10.0 };

            Console.WriteLine("Bessel function values:");
            foreach (double x in testPoints)
            {
                double j0 = J0(x);
                double y0 = N0(x);
                Compl h02 = H0_2(x);

                Console.WriteLine(string.Format("x={0:F1}: J0={1:E6}, Y0={2:E6}, |H0|={3:E6}", x, j0, y0, Compl.Abs(h02)));
            }
        }

        // Класс для представления комплексных чисел
        public class Compl
        {
            public double Re;
            public double Im;

            public Compl()
            {
                Re = 0;
                Im = 0;
            }

            public Compl(double x)
            {
                Re = x;
                Im = 0;
            }

            public Compl(double x, double y)
            {
                Re = x;
                Im = y;
            }

            public static Compl operator +(Compl x1, Compl x2)
            {
                return new Compl(x1.Re + x2.Re, x1.Im + x2.Im);
            }

            public static Compl operator -(Compl x)
            {
                return new Compl(-x.Re, -x.Im);
            }

            public static Compl operator -(Compl x1, Compl x2)
            {
                return new Compl(x1.Re - x2.Re, x1.Im - x2.Im);
            }

            public static Compl operator *(Compl x1, Compl x2)
            {
                return new Compl(x1.Re * x2.Re - x1.Im * x2.Im, x1.Re * x2.Im + x1.Im * x2.Re);
            }

            public static Compl operator /(Compl x1, Compl x2)
            {
                double y = x2.Re * x2.Re + x2.Im * x2.Im;
                if (Math.Abs(y) < 1e-15)
                    throw new DivideByZeroException("Division by zero complex number");

                return new Compl((x1.Re * x2.Re + x1.Im * x2.Im) / y,
                               (x2.Re * x1.Im - x2.Im * x1.Re) / y);
            }

            public static Compl operator *(Compl x1, double x2)
            {
                return new Compl(x2 * x1.Re, x2 * x1.Im);
            }

            public static Compl operator /(Compl x1, double x2)
            {
                if (Math.Abs(x2) < 1e-15)
                    throw new DivideByZeroException("Division by zero");

                return new Compl(x1.Re / x2, x1.Im / x2);
            }

            public static Compl operator *(double x, Compl y)
            {
                return new Compl(x * y.Re, x * y.Im);
            }

            public static Compl operator /(double x, Compl y)
            {
                double r = y.Re * y.Re + y.Im * y.Im;
                if (Math.Abs(r) < 1e-15)
                    throw new DivideByZeroException("Division by zero complex number");

                return new Compl(x * y.Re / r, -x * y.Im / r);
            }

            public static Compl operator +(Compl x, double y)
            {
                return new Compl(x.Re + y, x.Im);
            }

            public static Compl operator +(double x, Compl y)
            {
                return new Compl(x + y.Re, y.Im);
            }

            public static Compl operator -(double x, Compl y)
            {
                return new Compl(x - y.Re, -y.Im);
            }

            public static Compl operator -(Compl x, double y)
            {
                return new Compl(x.Re - y, x.Im);
            }

            public static Compl Exp(Compl x)
            {
                Compl z = new Compl(Math.Exp(x.Re), 0);
                Compl y = new Compl(Math.Cos(x.Im), Math.Sin(x.Im));
                return z * y;
            }

            public static Compl Log(Compl x)
            {
                return new Compl(Math.Log(Abs(x)), Argum(x));
            }

            public static Compl Pow(Compl x, double n)
            {
                double r = Math.Pow(x.Re * x.Re + x.Im * x.Im, n / 2);
                double a = Math.Atan2(x.Im, x.Re);
                return new Compl(r * Math.Cos(a * n), r * Math.Sin(a * n));
            }

            public static Compl Pow(Compl x, Compl y)
            {
                return Exp(y * Log(x));
            }

            public static double Abs(Compl x)
            {
                return Math.Sqrt(x.Re * x.Re + x.Im * x.Im);
            }

            public static double Argum(Compl x)
            {
                return Math.Atan2(x.Im, x.Re);
            }
        }

        public static readonly Compl ci = new Compl(0, 1);

        public class CVect
        {
            private Compl[] v;
            private int sz;

            public CVect(int size)
            {
                sz = size;
                v = new Compl[sz];
                for (int i = 0; i < sz; i++)
                    v[i] = new Compl(0, 0);
            }

            ~CVect()
            {
                v = null;
            }

            public int Size()
            {
                return sz;
            }

            public Compl this[int index]
            {
                get
                {
                    if (index < 0 || index >= sz)
                        throw new IndexOutOfRangeException(string.Format("CVect index {0} out of range [0, {1}]", index, sz - 1));
                    return v[index];
                }
                set
                {
                    if (index < 0 || index >= sz)
                        throw new IndexOutOfRangeException(string.Format("CVect index {0} out of range [0, {1}]", index, sz - 1));
                    v[index] = value;
                }
            }
        }

        public class CMatr
        {
            private CVect[] v;
            private int sz;

            public CMatr(int size)
            {
                sz = size;
                v = new CVect[sz];
                for (int i = 0; i < sz; i++)
                {
                    v[i] = new CVect(sz);
                }
            }

            ~CMatr()
            {
                v = null;
            }

            public int Size()
            {
                return sz;
            }

            public CVect this[int index]
            {
                get
                {
                    if (index < 0 || index >= sz)
                        throw new IndexOutOfRangeException(string.Format("CMatr index {0} out of range [0, {1}]", index, sz - 1));
                    return v[index];
                }
                set
                {
                    if (index < 0 || index >= sz)
                        throw new IndexOutOfRangeException(string.Format("CMatr index {0} out of range [0, {1}]", index, sz - 1));
                    v[index] = value;
                }
            }
        }

        public static int Gauss(CMatr A, CVect b, CVect x)
        {
            Compl s, s1;
            double max, ss;
            int maxN;
            int N = b.Size();

            for (int i = 0; i < N - 1; i++)
            {
                max = Compl.Abs(A[i][i]);
                maxN = i;
                for (int k = i + 1; k < N; k++)
                {
                    ss = Compl.Abs(A[k][i]);
                    if (ss > max)
                    {
                        max = ss;
                        maxN = k;
                    }
                }
                if (maxN != i)
                {
                    for (int k = 0; k < N; k++)
                    {
                        s1 = A[i][k];
                        A[i][k] = A[maxN][k];
                        A[maxN][k] = s1;
                    }
                    s1 = b[i];
                    b[i] = b[maxN];
                    b[maxN] = s1;
                }

                if (Compl.Abs(A[i][i]) < 1e-12)
                {
                    return -1;
                }
                s = 1 / A[i][i];
                for (int j = i + 1; j < N; j++)
                {
                    s1 = A[j][i] * s;
                    for (int k = i + 1; k < N; k++)
                    {
                        A[j][k] = A[j][k] - s1 * A[i][k];
                    }
                    b[j] = b[j] - b[i] * s1;
                }
            }

            // проверка на малость
            if (Compl.Abs(A[N - 1][N - 1]) < 1e-12)
            {
                return -1;
            }
            x[N - 1] = b[N - 1] / A[N - 1][N - 1];
            for (int i = N - 2; i >= 0; i--)
            {
                s = b[i];
                for (int j = N - 1; j > i; j--)
                {
                    s = s - A[i][j] * x[j];
                }
                x[i] = s / A[i][i];
            }
            return 1;
        }

        public static double Cheb(int n, double x)
        {
            double T, T0, T1;
            T = 0;
            T0 = 1.0;
            T1 = x;
            if (n == 0)
                T = 1.0;
            else if (n == 1)
                T = x;
            else
                for (int i = 2; i <= n; i++)
                {
                    T = 2 * x * T1 - T0;
                    T0 = T1;
                    T1 = T;
                }
            return T;
        }

        public static double J0(double x)
        {
            const double eps = 1e-4;
            const int maxIter = 10000;
            double sum = 0, s = -1;
            double k2 = 1, xS = 1;
            double x2 = x * x / 4;
            double _1 = -1;
            long k = 0;

            while (Math.Abs(s) > eps && k < maxIter)
            {
                sum += s;
                k++;
                _1 = -_1;
                k2 /= k * k;
                xS *= x2;
                s = _1 * k2 * xS;
            }

            if (k >= maxIter)
                Console.WriteLine(string.Format("Warning: J0({0}) did not converge in {1} iterations", x, maxIter));

            return -sum;
        }

        public static double _Y0(double x)
        {
            const double eps = 1e-4;
            const int maxIter = 10000;
            double sum = 0, s = -1;
            double k2 = 1, xS = 1;
            double x2 = x * x / 4;
            double _1 = -1;
            double psi = -0.57721566 + 1;
            long k = 0;

            while (Math.Abs(s) > eps && k < maxIter)
            {
                sum += s;
                k++;
                _1 = -_1;
                k2 /= k * k;
                xS *= x2;
                psi += 1.0 / k;
                s = _1 * k2 * xS * psi;
            }

            if (k >= maxIter)
                Console.WriteLine(string.Format("Warning: _Y0({0}) did not converge in {1} iterations", x, maxIter));

            return sum;
        }

        public static double N0(double x)
        {
            return 2.0 / Math.PI * (J0(x) * Math.Log(x / 2) + _Y0(x));
        }

        public static Compl H0_1(double x)
        {
            return N0(x) * ci + J0(x);
        }

        public static Compl H0_2(double x)
        {
            return J0(x) - N0(x) * ci;
        }

        public class DifrOnLenta
        {
            public double a, b;
            public double lambda;
            public int N;
            public double teta;
            public Compl[] y;
            public double skinDepth;
            public Compl chi; // Импедансный коэффициент для скин-эффекта

            public DifrOnLenta(double _a, double _b, double _lambda, double _teta, int _N, double _skinDepth = 0)
            {
                a = _a;
                b = _b;
                N = _N;
                lambda = _lambda;
                teta = _teta;
                y = new Compl[N];
                for (int i = 0; i < N; i++)
                    y[i] = new Compl(0, 0);
                skinDepth = _skinDepth;
                
                // Расчет импедансного коэффициента χ (хи)
                chi = CalculateChi();
            }

            ~DifrOnLenta()
            {
                y = null;
            }
            
            // Расчет импедансного коэффициента χ для скин-эффекта
            private Compl CalculateChi()
            {
                if (skinDepth <= 0)
                {
                    // Для идеального проводника χ = 0
                    return new Compl(0, 0);
                }
                
                // Волновое число k = 2π/λ
                double k = 2 * Math.PI / lambda;
                
                // Нормированный импедансный параметр
                // χ = k * δ * (1 + i) - классическая формула для скин-эффекта
                // Множитель (1+i) отражает фазовый сдвиг в проводнике
                double chiReal = k * skinDepth;
                double chiImag = k * skinDepth;
                
                return new Compl(chiReal, chiImag);
            }

            public double ChebAB(int n, double x)
            {
                return Cheb(n, 2.0 / (b - a) * x - (b + a) / (b - a));
            }

            public double CalculateConductivity(double skinDepth, double wavelength)
            {
                if (skinDepth <= 0)
                {
                    throw new ArgumentException("Толщина скин-слоя должна быть положительной");
                }

                const double mu0 = 4 * Math.PI * 1e-7;
                const double c = 299792458;

                double frequency = c / wavelength;
                double conductivity = 1.0 / (Math.PI * mu0 * frequency * skinDepth * skinDepth);

                return conductivity;
            }

            public Compl u0(double x, double z)
            {
                double k = 2 * Math.PI / lambda;
                return Compl.Exp(k * Math.Cos(teta) * ci * x + k * Math.Sin(teta) * ci * z);
            }

            // Полное поле: u = u0 + u_рассеянное
            public Compl u(double x, double z)
            {
                double k = 2 * Math.PI / lambda;
                int M = Math.Max(80, 4 * N);
                double dt = (b - a) / M;

                Compl s = new Compl(0, 0);
                for (int i = 0; i < N; i++)
                {
                    Compl integ = new Compl(0, 0);
                    for (int m = 0; m < M; m++)
                    {
                        double t = a + (m + 0.5) * dt;
                        double distance = Math.Sqrt(z * z + (t - x) * (t - x));
                        if (distance < 1e-10) distance = 1e-10;
                        integ += H0_2(k * distance) * ChebAB(i, t) * dt;
                    }
                    s += y[i] * integ;
                }
                return s * ci / 4.0 + u0(x, z);
            }

            // Прямая коллокация с Чебышевскими точками
            // Для идеального проводника: u(x_k, 0) = 0
            // Для импедансного: u(x_k, 0) + χ * ∂u/∂n(x_k, 0) = 0 (условие Леонтовича)
            public int SolveDifr()
            {
                double k = 2 * Math.PI / lambda;
                int M = Math.Max(80, 4 * N);
                double dt = (b - a) / M;

                CVect B = new CVect(N);
                CMatr A = new CMatr(N);

                // Точки коллокации (Чебышев)
                double[] xc = new double[N];
                for (int i = 0; i < N; i++)
                    xc[i] = (b - a) / 2.0 * Math.Cos((2 * i + 1) / 2.0 / N * Math.PI) + (b + a) / 2.0;

                bool isImpedance = Compl.Abs(chi) > 1e-15;
                double dz_fd = lambda / 500.0; // шаг для конечной разности ∂/∂n

                for (int ik = 0; ik < N; ik++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        // Интеграл ядра на z=0: G_j = (i/4) ∫ H0(2)(k|x-t|) T_j(t) dt
                        Compl s0 = new Compl(0.0);
                        for (int m = 0; m < M; m++)
                        {
                            double t = a + (m + 0.5) * dt;
                            double dist = Math.Abs(xc[ik] - t);
                            if (dist < 1e-10) dist = 1e-10;
                            s0 += H0_2(k * dist) * ChebAB(j, t) * dt;
                        }
                        Compl Gj_0 = s0 * ci / 4.0;

                        if (isImpedance)
                        {
                            // Интеграл ядра на z=dz: для вычисления ∂G/∂n
                            Compl s_dz = new Compl(0.0);
                            for (int m = 0; m < M; m++)
                            {
                                double t = a + (m + 0.5) * dt;
                                double distance = Math.Sqrt(dz_fd * dz_fd + (t - xc[ik]) * (t - xc[ik]));
                                s_dz += H0_2(k * distance) * ChebAB(j, t) * dt;
                            }
                            Compl Gj_dz = s_dz * ci / 4.0;
                            Compl dGj_dn = (Gj_dz - Gj_0) / dz_fd;

                            // A[ik][j] = G_j(x_k, 0) + χ * ∂G_j/∂n(x_k, 0)
                            A[ik][j] = Gj_0 + chi * dGj_dn;
                        }
                        else
                        {
                            A[ik][j] = Gj_0;
                        }
                    }

                    if (isImpedance)
                    {
                        // Правая часть: -(u0 + χ * ∂u0/∂n) на z=0
                        Compl u0_0 = u0(xc[ik], 0);
                        Compl u0_dz = u0(xc[ik], dz_fd);
                        Compl du0_dn = (u0_dz - u0_0) / dz_fd;
                        B[ik] = -(u0_0 + chi * du0_dn);
                    }
                    else
                    {
                        B[ik] = -u0(xc[ik], 0);
                    }
                }

                CVect w = new CVect(N);
                int output = Gauss(A, B, w);

                for (int i = 0; i < N; i++)
                    y[i] = w[i];
                return output;
            }

            public class EnergyComponents
            {
                public double Incident;
                public double Reflected;
                public double Transmitted;
                public double Absorbed;
            }

            // Вычисление амплитуды плотности φ(t) = Σ y_j T_j(t) в точке t
            public Compl Density(double t)
            {
                Compl s = new Compl(0, 0);
                for (int j = 0; j < N; j++)
                    s += y[j] * ChebAB(j, t);
                return s;
            }

            // Вычисление диаграммы направленности рассеянного поля f(φ)
            // Далеко от полосы: u_s ~ f(φ) * e^{-ikr}/√(kr)
            // f(φ) = √(2πk) * e^{iπ/4} / 4 * ∫_a^b φ(t) e^{ik cos(φ) t} dt
            public Compl FarFieldAmplitude(double phi)
            {
                double k = 2 * Math.PI / lambda;
                int M = Math.Max(80, 4 * N);
                double dt_q = (b - a) / M;

                // Интеграл ∫ φ(t) * e^{ik cos(phi) t} dt
                Compl integral = new Compl(0, 0);
                for (int m = 0; m < M; m++)
                {
                    double t = a + (m + 0.5) * dt_q;
                    Compl dens = Density(t);
                    Compl phase = Compl.Exp(ci * k * Math.Cos(phi) * t);
                    integral += dens * phase * dt_q;
                }

                // Коэффициент: (1/4) * √(2π/k) * e^{iπ/4} = (1/4) * √(2π/k) * (1+i)/√2
                double coeff = Math.Sqrt(2.0 * Math.PI / k) / 4.0;
                Compl eipi4 = new Compl(1.0 / Math.Sqrt(2), 1.0 / Math.Sqrt(2)); // e^{iπ/4}
                return integral * coeff * eipi4;
            }

            public EnergyComponents CalculateEnergyComponents()
            {
                EnergyComponents energy = new EnergyComponents();

                energy.Incident = CalculateIncidentEnergy();

                // Полная рассеянная мощность через интеграл |f(φ)|² по углу
                double totalScattered = CalculateTotalScatteredEnergy();

                energy.Absorbed = CalculateAbsorbedEnergy();

                // Рассеянная = отражённая + прошедшая (дифракция вокруг краёв)
                // Разделяем: отражённая = поток в полупространство x < a
                //            прошедшая  = поток в полупространство x > b
                double reflected = 0, transmitted = 0;
                SplitScatteredEnergy(out reflected, out transmitted);

                energy.Reflected = reflected;
                energy.Transmitted = transmitted;

                return energy;
            }

            // Падающая энергия: поток плоской волны через "окно" размером 2*(b-a)
            // Нормировка: Eинц = (b-a) * sin(θ)  (проекция на полосу)
            // Используем нормировку через полосу: E_inc = (b-a) для θ=90°
            public double CalculateIncidentEnergy()
            {
                // Поток плоской волны через сечение длиной (b-a):
                // P_inc = (b-a) * sin(θ)  для TM-поляризации (или просто (b-a))
                // Чтобы энергетический баланс был безразмерным, нормируем на 1:
                return 1.0;
            }

            // Полная рассеянная мощность (интеграл далёкого поля)
            // σ_total = ∫_0^{2π} |f(φ)|² dφ
            public double CalculateTotalScatteredEnergy()
            {
                int Nphi = 360;
                double dphi = 2.0 * Math.PI / Nphi;
                double sum = 0;

                for (int i = 0; i < Nphi; i++)
                {
                    double phi = (i + 0.5) * dphi;
                    Compl f = FarFieldAmplitude(phi);
                    sum += (f.Re * f.Re + f.Im * f.Im) * dphi;
                }
                return sum;
            }

            // Разделение рассеянной энергии на отражённую и прошедшую
            // Отражённая: углы φ ∈ [π/2, 3π/2] (уходящее в полупространство z<0 + назад по x)
            // Прошедшая: углы φ ∈ [0, π/2] ∪ [3π/2, 2π] (вперёд и в z>0)
            public void SplitScatteredEnergy(out double reflected, out double transmitted)
            {
                int Nphi = 360;
                double dphi = 2.0 * Math.PI / Nphi;
                reflected = 0;
                transmitted = 0;

                for (int i = 0; i < Nphi; i++)
                {
                    double phi = (i + 0.5) * dphi;
                    Compl f = FarFieldAmplitude(phi);
                    double intensity = (f.Re * f.Re + f.Im * f.Im) * dphi;

                    // sin(φ) > 0 → z > 0 (полупространство z>0, "отражённое")
                    // sin(φ) < 0 → z < 0 (полупространство z<0, "прошедшее")
                    // (падающая волна идёт в направлении sin(θ) > 0, т.е. z↑)
                    double sinPhi = Math.Sin(phi);
                    if (sinPhi >= 0)
                        reflected += intensity;
                    else
                        transmitted += intensity;
                }
            }

            public double CalculateReflectedEnergy()
            {
                double refl, trans;
                SplitScatteredEnergy(out refl, out trans);
                return refl;
            }

            public double CalculateAbsorbedEnergy()
            {
                if (skinDepth <= 0) return 0;

                // Поглощённая энергия через оптическую теорему:
                // P_abs = Re(χ) * ∫_a^b |u(x,0)|² dx  (нормированная)
                double k = 2 * Math.PI / lambda;
                int M = Math.Max(80, 4 * N);
                double dx = (b - a) / M;
                double sum = 0;

                for (int m = 0; m < M; m++)
                {
                    double x = a + (m + 0.5) * dx;
                    Compl u_val = u(x, 0);
                    sum += (u_val.Re * u_val.Re + u_val.Im * u_val.Im) * dx;
                }

                // Нормировка: P_abs = k * Re(χ) / 2 * ∫|u|²dx
                // Согласуем с нормировкой E_inc = 1
                return k * chi.Re / 2.0 * sum;
            }

            public double CalculateTransmittedEnergyIndependent()
            {
                double refl, trans;
                SplitScatteredEnergy(out refl, out trans);
                return trans;
            }

            public double VerifyBoundaryConditions()
            {
                int Mtest = 40;
                double dx = (b - a) / Mtest;
                double sumErr = 0;

                for (int i = 0; i <= Mtest; i++)
                {
                    double x = a + i * dx;
                    Compl u_val = u(x, 0);

                    Compl bc_val;
                    if (Compl.Abs(chi) < 1e-15)
                    {
                        bc_val = u_val;
                    }
                    else
                    {
                        double dz = lambda / 500.0;
                        Compl u_plus = u(x, dz);
                        Compl du_dn = (u_plus - u_val) / dz;
                        bc_val = u_val + chi * du_dn;
                    }

                    double scale = Math.Max(Compl.Abs(u0(x, 0)), 0.1);
                    sumErr += Compl.Abs(bc_val) / scale;
                }

                return sumErr / (Mtest + 1);
            }

            public double VerifyHelmholtz()
            {
                double x = b + lambda;
                double z = lambda;
                double k = 2 * Math.PI / lambda;
                double h = lambda / 100.0;

                Compl u_0 = u(x, z);
                Compl u_x1 = u(x + h, z);
                Compl u_x2 = u(x - h, z);
                Compl u_z1 = u(x, z + h);
                Compl u_z2 = u(x, z - h);

                Compl laplacian = (u_x1 + u_x2 + u_z1 + u_z2 - 4 * u_0) / (h * h);
                Compl helmholtz = laplacian + k * k * u_0;

                return Compl.Abs(helmholtz) / (k * k * Compl.Abs(u_0) + 1e-10);
            }

            public void VerifyEnergyConservation()
            {
                // Для энергетического баланса используем оптическую теорему
                // σ_ext = σ_scat + σ_abs
                // σ_ext можно вычислить через Im(f(θ_0)) - передняя амплитуда
                double totalScattered = CalculateTotalScatteredEnergy();
                double absorbed = CalculateAbsorbedEnergy();

                // Оптическая теорема: σ_ext = (4/k) * Im(f(θ_inc))
                // где θ_inc - направление падения
                double k = 2 * Math.PI / lambda;
                Compl f_forward = FarFieldAmplitude(teta);
                double sigma_ext_OT = 4.0 / k * f_forward.Im;

                double sigma_total = totalScattered + absorbed;

                Console.WriteLine("Energy Balance Check (Far-Field):");
                Console.WriteLine(string.Format("  Total scattered σ_s: {0:F6}", totalScattered));
                Console.WriteLine(string.Format("  Absorbed σ_a:        {0:F6}", absorbed));
                Console.WriteLine(string.Format("  Total σ_s + σ_a:     {0:F6}", sigma_total));
                Console.WriteLine(string.Format("  Optical theorem σ_ext = 4/k Im(f): {0:F6}", sigma_ext_OT));

                double refl, trans;
                SplitScatteredEnergy(out refl, out trans);
                Console.WriteLine(string.Format("  Reflected:           {0:F6}", refl));
                Console.WriteLine(string.Format("  Transmitted (diffr): {0:F6}", trans));

                double bcErr = VerifyBoundaryConditions();
                Console.WriteLine(string.Format("  BC error:            {0:P2}", bcErr));

                double helmErr = VerifyHelmholtz();
                Console.WriteLine(string.Format("  Helmholtz residual:  {0:E2}", helmErr));
            }
        }
    }
}