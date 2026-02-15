using System;

class Compl
{
    public double Re, Im;
    public Compl() { Re = 0; Im = 0; }
    public Compl(double x) { Re = x; Im = 0; }
    public Compl(double x, double y) { Re = x; Im = y; }
    public static Compl operator +(Compl a, Compl b) { return new Compl(a.Re+b.Re, a.Im+b.Im); }
    public static Compl operator -(Compl a, Compl b) { return new Compl(a.Re-b.Re, a.Im-b.Im); }
    public static Compl operator -(Compl a) { return new Compl(-a.Re, -a.Im); }
    public static Compl operator *(Compl a, Compl b) { return new Compl(a.Re*b.Re-a.Im*b.Im, a.Re*b.Im+a.Im*b.Re); }
    public static Compl operator /(Compl a, Compl b) { double d=b.Re*b.Re+b.Im*b.Im; return new Compl((a.Re*b.Re+a.Im*b.Im)/d,(b.Re*a.Im-b.Im*a.Re)/d); }
    public static Compl operator *(Compl a, double b) { return new Compl(b*a.Re, b*a.Im); }
    public static Compl operator *(double a, Compl b) { return new Compl(a*b.Re, a*b.Im); }
    public static Compl operator /(Compl a, double b) { return new Compl(a.Re/b, a.Im/b); }
    public static Compl operator /(double a, Compl b) { double d=b.Re*b.Re+b.Im*b.Im; return new Compl(a*b.Re/d,-a*b.Im/d); }
    public static Compl operator +(Compl a, double b) { return new Compl(a.Re+b, a.Im); }
    public static Compl operator +(double a, Compl b) { return new Compl(a+b.Re, b.Im); }
    public static Compl operator -(Compl a, double b) { return new Compl(a.Re-b, a.Im); }
    public static Compl operator -(double a, Compl b) { return new Compl(a-b.Re, -b.Im); }
    public static Compl Exp(Compl x) { return new Compl(Math.Exp(x.Re)*Math.Cos(x.Im), Math.Exp(x.Re)*Math.Sin(x.Im)); }
    public static double Abs(Compl x) { return Math.Sqrt(x.Re*x.Re+x.Im*x.Im); }
    public Compl Conj() { return new Compl(Re, -Im); }
}

class TestHypotheses
{
    static Compl ci = new Compl(0,1);

    static double J0(double x) {
        double sum=0,s=-1,k2=1,xS=1,x2=x*x/4,sign=-1;
        int k=0;
        while(Math.Abs(s)>1e-10 && k<10000) { sum+=s; k++; sign=-sign; k2/=k*k; xS*=x2; s=sign*k2*xS; }
        return -sum;
    }

    static double _Y0(double x) {
        double sum=0,s=-1,k2=1,xS=1,x2=x*x/4,sign=-1,psi=-0.57721566+1;
        int k=0;
        while(Math.Abs(s)>1e-10 && k<10000) { sum+=s; k++; sign=-sign; k2/=k*k; xS*=x2; psi+=1.0/k; s=sign*k2*xS*psi; }
        return sum;
    }

    static double N0(double x) { return 2.0/Math.PI*(J0(x)*Math.Log(x/2)+_Y0(x)); }
    static Compl H0_2(double x) { return new Compl(J0(x), -N0(x)); }

    static double Cheb(int n, double x) {
        if(n==0) return 1; if(n==1) return x;
        double T0=1,T1=x,T=0;
        for(int i=2;i<=n;i++){T=2*x*T1-T0;T0=T1;T1=T;}
        return T;
    }

    static double a=-1, b_=1, lambda=1.0;
    static int N_=10;
    static double teta;
    static Compl[] y;

    static double ChebAB(int n, double x) { return Cheb(n, 2.0/(b_-a)*x-(b_+a)/(b_-a)); }

    static Compl u0(double x, double z) {
        double k=2*Math.PI/lambda;
        return Compl.Exp(k*Math.Cos(teta)*ci*x + k*Math.Sin(teta)*ci*z);
    }

    static Compl r_kernel(double t, double x) {
        double k=2*Math.PI/lambda;
        Compl s=new Compl(J0(k*Math.Abs(t-x)));
        if(Math.Abs(t-x)>1e-8)
            s=Math.PI*ci/2*s+(s-1.0)*Math.Log(k*Math.Abs(t-x)/2.0);
        else
            s=Math.PI*ci/2.0*s;
        s=s+Math.Log(k/2.0)+_Y0(k*Math.Abs(t-x));
        return -s;
    }

    static Compl u_field(double x, double z) {
        int M=Math.Max(20, 2*N_);
        double k=2*Math.PI/lambda;
        double[] v=new double[M];
        for(int m=0;m<M;m++) v[m]=(b_-a)/2.0*Math.Cos((2*m+1)/2.0/M*Math.PI)+(b_+a)/2.0;
        Compl s=new Compl(0,0);
        for(int i=0;i<N_;i++){
            Compl Int=new Compl(0,0);
            for(int m=0;m<M;m++){
                double d=Math.Sqrt(z*z+(v[m]-x)*(v[m]-x));
                if(d<1e-10) d=1e-10;
                Int=Int+H0_2(k*d)*ChebAB(i,v[m]);
            }
            Int=Int*(Math.PI/M);
            s=s+y[i]*Int;
        }
        return s*ci/4.0+u0(x,z);
    }

    static int Gauss(Compl[,] A, Compl[] b, Compl[] x, int N) {
        for(int i=0;i<N-1;i++){
            double max=Compl.Abs(A[i,i]); int maxN=i;
            for(int k=i+1;k<N;k++){double ss=Compl.Abs(A[k,i]);if(ss>max){max=ss;maxN=k;}}
            if(maxN!=i){for(int k=0;k<N;k++){var t=A[i,k];A[i,k]=A[maxN,k];A[maxN,k]=t;}var t2=b[i];b[i]=b[maxN];b[maxN]=t2;}
            if(Compl.Abs(A[i,i])<1e-12) return -1;
            var s=1.0/A[i,i];
            for(int j=i+1;j<N;j++){var s1=A[j,i]*s;for(int k=i+1;k<N;k++) A[j,k]=A[j,k]-s1*A[i,k];b[j]=b[j]-b[i]*s1;}
        }
        if(Compl.Abs(A[N-1,N-1])<1e-12) return -1;
        x[N-1]=b[N-1]/A[N-1,N-1];
        for(int i=N-2;i>=0;i--){var s=b[i];for(int j=N-1;j>i;j--) s=s-A[i,j]*x[j];x[i]=s/A[i,i];}
        return 1;
    }

    static int SolveDifr() {
        int M=Math.Max(20, 2*N_);
        var A=new Compl[N_,N_]; var B=new Compl[N_];
        double[] xp=new double[M];
        for(int m=0;m<M;m++) xp[m]=(b_-a)/2.0*Math.Cos((2*m+1)/2.0/M*Math.PI)+(b_+a)/2.0;
        for(int k=0;k<N_;k++){
            for(int j=0;j<N_;j++){
                var s=new Compl(0);
                for(int m=0;m<M;m++) for(int n=0;n<M;n++) s=s+r_kernel(xp[n],xp[m])*ChebAB(j,xp[n])*ChebAB(k,xp[m]);
                s=s*(Math.PI*Math.PI/M/M);
                A[k,j]=s;
            }
            var sb=new Compl(0);
            for(int m=0;m<M;m++) sb=sb+(-2*Math.PI*u0(xp[m],0))*ChebAB(k,xp[m]);
            B[k]=sb*(Math.PI/M);
            Compl reg; if(k==0) reg=new Compl(Math.PI*Math.PI*Math.Log(b_-a),0); else reg=new Compl(Math.PI*Math.PI/2.0/(k+1),0);
            A[k,k]=A[k,k]+reg;
        }
        y=new Compl[N_];
        return Gauss(A,B,y,N_);
    }

    // Отраженная энергия через поток Пойнтинга рассеянного поля
    static double CalcReflectedPoynting() {
        double k=2*Math.PI/lambda;
        double x_meas=a-lambda;
        int Nz=200;
        double z_max=5.0*(b_-a);
        double dz=2.0*z_max/Nz;
        double flux=0;
        double h=lambda/200.0;
        for(int i=0;i<Nz;i++){
            double z=-z_max+(i+0.5)*dz;
            Compl us=u_field(x_meas,z)-u0(x_meas,z);
            Compl us_dx=(u_field(x_meas+h,z)-u0(x_meas+h,z))-(u_field(x_meas-h,z)-u0(x_meas-h,z));
            us_dx=us_dx/(2*h);
            double Sx=-us.Conj().Re*us_dx.Im+us.Conj().Im*us_dx.Re;
            flux+=Sx*dz;
        }
        return -flux/k;
    }

    // Прошедшая энергия через поток Пойнтинга полного поля
    static double CalcTransmittedPoynting() {
        double k=2*Math.PI/lambda;
        double x_meas=b_+lambda;
        int Nz=200;
        double z_max=5.0*(b_-a);
        double dz=2.0*z_max/Nz;
        double flux=0;
        double h=lambda/200.0;
        for(int i=0;i<Nz;i++){
            double z=-z_max+(i+0.5)*dz;
            Compl ut=u_field(x_meas,z);
            Compl ut_dx=(u_field(x_meas+h,z)-u_field(x_meas-h,z))/(2*h);
            double Sx=-ut.Conj().Re*ut_dx.Im+ut.Conj().Im*ut_dx.Re;
            flux+=Sx*dz;
        }
        return flux/k;
    }

    static double CalcIncidentFlux() {
        double k=2*Math.PI/lambda;
        double z_max=5.0*(b_-a);
        return Math.Abs(Math.Cos(teta))*2.0*z_max;
    }

    static double VerifyBC() {
        int M=40;
        double dx=(b_-a)/M;
        double sumErr=0;
        for(int i=0;i<=M;i++){
            double x=a+i*dx;
            Compl uv=u_field(x,0);
            sumErr+=Compl.Abs(uv)/Math.Max(Compl.Abs(u0(x,0)),0.1);
        }
        return sumErr/(M+1);
    }

    static double VerifyHelmholtz() {
        double x=b_+lambda; double z=lambda;
        double k=2*Math.PI/lambda; double h=lambda/100.0;
        Compl u0v=u_field(x,z);
        Compl lap=(u_field(x+h,z)+u_field(x-h,z)+u_field(x,z+h)+u_field(x,z-h)-4*u0v)/(h*h);
        Compl helm=lap+k*k*u0v;
        return Compl.Abs(helm)/(k*k*Compl.Abs(u0v)+1e-10);
    }

    static void Main() {
        Console.WriteLine("=== COMPREHENSIVE TEST: Energy + BC at various angles ===");
        Console.WriteLine("{0,-8} {1,-12} {2,-12} {3,-12} {4,-12} {5,-10} {6,-10}",
            "Angle", "E_inc", "E_ref", "E_trans", "Sum/Inc", "BC_err%", "Helm_err");
        Console.WriteLine(new string('-',80));

        double[] angles = { 10, 20, 30, 45, 60, 70, 80 };
        foreach(double ang in angles) {
            teta = ang / 180.0 * Math.PI;
            if(SolveDifr() != 1) { Console.WriteLine("{0,-8} FAILED", ang); continue; }

            double Einc = CalcIncidentFlux();
            double Eref = CalcReflectedPoynting();
            double Etrans = CalcTransmittedPoynting();
            double sum = Eref + Etrans;
            double bcErr = VerifyBC();
            double helmErr = VerifyHelmholtz();

            Console.WriteLine("{0,-8:F0} {1,-12:F4} {2,-12:F4} {3,-12:F4} {4,-12:P2} {5,-10:P2} {6,-10:E2}",
                ang, Einc, Eref, Etrans, sum/Einc, bcErr, helmErr);
        }

        Console.WriteLine("\n=== CONVERGENCE TEST ===");
        teta = Math.PI/4;
        Console.WriteLine("{0,-5} {1,-12} {2,-12} {3,-12} {4,-12} {5,-10}",
            "N", "E_ref", "E_trans", "Sum/Inc", "BC_err%", "Helm");
        Console.WriteLine(new string('-',65));
        int[] Ns = {5, 8, 10, 12, 15, 20};
        foreach(int n in Ns) {
            N_ = n;
            if(SolveDifr() != 1) { Console.WriteLine("{0,-5} FAILED", n); continue; }
            double Einc=CalcIncidentFlux();
            double Eref=CalcReflectedPoynting();
            double Etrans=CalcTransmittedPoynting();
            double bcErr=VerifyBC();
            double helmErr=VerifyHelmholtz();
            Console.WriteLine("{0,-5} {1,-12:F4} {2,-12:F4} {3,-12:P2} {4,-10:P2} {5,-10:E2}",
                n, Eref, Etrans, (Eref+Etrans)/Einc, bcErr, helmErr);
        }
    }
}
