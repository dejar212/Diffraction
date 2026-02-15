// Интеграционный тест: проверка BC, Гельмгольца, энергии при разных углах и N
// Компиляция: csc /reference:System.dll TestIntegration.cs
// (или как консольный проект в Visual Studio)

using System;

// Копируем необходимые классы из Program.cs для standalone-теста
// В реальности они уже в Diffraction.Program

class TestIntegration
{
    static void Main()
    {
        Console.WriteLine("=================================================================");
        Console.WriteLine("  INTEGRATION TEST: BC + Helmholtz + Energy at multiple angles");
        Console.WriteLine("=================================================================");
        Console.WriteLine();

        double a = -1, b_strip = 1, lambda = 1.0;
        double[] angles_deg = { 10, 20, 30, 45, 60, 75, 80 };
        int[] Ns = { 8, 10, 12, 16 };

        Console.WriteLine("=== TEST 1: Ideal conductor (skinDepth=0) ===");
        Console.WriteLine("{0,-8} {1,-5} {2,-12} {3,-12} {4,-12} {5,-12} {6,-12}",
            "Angle", "N", "BC_err%", "Helm_err", "σ_scatter", "σ_OT", "OT_err%");
        Console.WriteLine(new string('-', 75));

        foreach (double angle_deg in angles_deg)
        {
            double angle = angle_deg / 180.0 * Math.PI;
            foreach (int N in Ns)
            {
                var solver = new Diffraction.Program.DifrOnLenta(a, b_strip, lambda, angle, N, 0);
                int result = solver.SolveDifr();
                if (result != 1)
                {
                    Console.WriteLine("{0,-8} {1,-5} FAILED (Gauss)", angle_deg, N);
                    continue;
                }

                double bcErr = solver.VerifyBoundaryConditions();
                double helmErr = solver.VerifyHelmholtz();
                double sigma_s = solver.CalculateTotalScatteredEnergy();

                // Оптическая теорема
                double k = 2 * Math.PI / lambda;
                var f_fwd = solver.FarFieldAmplitude(angle);
                double sigma_OT = 4.0 / k * f_fwd.Im;
                double otErr = Math.Abs(sigma_OT - sigma_s) / Math.Max(Math.Abs(sigma_OT), 1e-10);

                Console.WriteLine("{0,-8} {1,-5} {2,-12:P2} {3,-12:E2} {4,-12:F4} {5,-12:F4} {6,-12:P2}",
                    angle_deg, N, bcErr, helmErr, sigma_s, sigma_OT, otErr);
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== TEST 2: Impedance conductor (skinDepth=0.1) ===");
        Console.WriteLine("{0,-8} {1,-5} {2,-12} {3,-12} {4,-12} {5,-12} {6,-12} {7,-12}",
            "Angle", "N", "BC_err%", "Helm_err", "σ_scatter", "σ_absorb", "σ_total", "σ_OT");
        Console.WriteLine(new string('-', 90));

        double skinDepth = 0.1;
        foreach (double angle_deg in angles_deg)
        {
            double angle = angle_deg / 180.0 * Math.PI;
            int N = 12;
            var solver = new Diffraction.Program.DifrOnLenta(a, b_strip, lambda, angle, N, skinDepth);
            int result = solver.SolveDifr();
            if (result != 1)
            {
                Console.WriteLine("{0,-8} {1,-5} FAILED", angle_deg, N);
                continue;
            }

            double bcErr = solver.VerifyBoundaryConditions();
            double helmErr = solver.VerifyHelmholtz();
            double sigma_s = solver.CalculateTotalScatteredEnergy();
            double sigma_a = solver.CalculateAbsorbedEnergy();
            double sigma_total = sigma_s + sigma_a;

            double k = 2 * Math.PI / lambda;
            var f_fwd = solver.FarFieldAmplitude(angle);
            double sigma_OT = 4.0 / k * f_fwd.Im;

            Console.WriteLine("{0,-8} {1,-5} {2,-12:P2} {3,-12:E2} {4,-12:F4} {5,-12:F4} {6,-12:F4} {7,-12:F4}",
                angle_deg, N, bcErr, helmErr, sigma_s, sigma_a, sigma_total, sigma_OT);
        }

        Console.WriteLine();
        Console.WriteLine("=== TEST 3: Field on strip (should be ~0 for ideal conductor) ===");
        {
            var solver = new Diffraction.Program.DifrOnLenta(a, b_strip, lambda, Math.PI / 4, 12, 0);
            solver.SolveDifr();

            Console.WriteLine("x         |u(x,0)|     u(x,0).Re    u(x,0).Im");
            Console.WriteLine(new string('-', 55));
            for (int i = 0; i <= 10; i++)
            {
                double x = a + i * (b_strip - a) / 10.0;
                var u_val = solver.u(x, 0);
                Console.WriteLine("{0,8:F3}  {1,10:E3}  {2,12:E3}  {3,12:E3}",
                    x, Diffraction.Program.Compl.Abs(u_val), u_val.Re, u_val.Im);
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== TEST 4: Convergence test (N=4,6,8,10,12,16,20) ===");
        {
            int[] Ns_conv = { 4, 6, 8, 10, 12, 16, 20 };
            double angle = Math.PI / 4;
            Console.WriteLine("{0,-6} {1,-12} {2,-12} {3,-12}",
                "N", "BC_err%", "Helm_err", "σ_scatter");
            Console.WriteLine(new string('-', 48));

            foreach (int N in Ns_conv)
            {
                var solver = new Diffraction.Program.DifrOnLenta(a, b_strip, lambda, angle, N, 0);
                int result = solver.SolveDifr();
                if (result != 1)
                {
                    Console.WriteLine("{0,-6} FAILED", N);
                    continue;
                }
                double bcErr = solver.VerifyBoundaryConditions();
                double helmErr = solver.VerifyHelmholtz();
                double sigma_s = solver.CalculateTotalScatteredEnergy();
                Console.WriteLine("{0,-6} {1,-12:P2} {2,-12:E2} {3,-12:F4}",
                    N, bcErr, helmErr, sigma_s);
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== TEST 5: Energy split (reflected vs transmitted) ===");
        {
            double angle = Math.PI / 4;
            var solver = new Diffraction.Program.DifrOnLenta(a, b_strip, lambda, angle, 12, 0);
            solver.SolveDifr();

            double refl, trans;
            solver.SplitScatteredEnergy(out refl, out trans);
            double total = refl + trans;

            Console.WriteLine("Angle=45°, N=12, ideal conductor:");
            Console.WriteLine("  Reflected:   {0:F6} ({1:P2})", refl, refl / total);
            Console.WriteLine("  Transmitted: {0:F6} ({1:P2})", trans, trans / total);
            Console.WriteLine("  Total:       {0:F6}", total);
        }

        Console.WriteLine();
        Console.WriteLine("=== ALL TESTS COMPLETE ===");
    }
}
