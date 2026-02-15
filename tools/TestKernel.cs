using System;

class Compl
{
    public double Re, Im;
    public Compl() { Re=0;Im=0; }
    public Compl(double x) { Re=x;Im=0; }
    public Compl(double x,double y) { Re=x;Im=y; }
    public static Compl operator+(Compl a,Compl b){return new Compl(a.Re+b.Re,a.Im+b.Im);}
    public static Compl operator-(Compl a,Compl b){return new Compl(a.Re-b.Re,a.Im-b.Im);}
    public static Compl operator-(Compl a){return new Compl(-a.Re,-a.Im);}
    public static Compl operator*(Compl a,Compl b){return new Compl(a.Re*b.Re-a.Im*b.Im,a.Re*b.Im+a.Im*b.Re);}
    public static Compl operator/(Compl a,Compl b){double d=b.Re*b.Re+b.Im*b.Im;return new Compl((a.Re*b.Re+a.Im*b.Im)/d,(b.Re*a.Im-b.Im*a.Re)/d);}
    public static Compl operator*(Compl a,double b){return new Compl(b*a.Re,b*a.Im);}
    public static Compl operator*(double a,Compl b){return new Compl(a*b.Re,a*b.Im);}
    public static Compl operator/(Compl a,double b){return new Compl(a.Re/b,a.Im/b);}
    public static Compl operator+(Compl a,double b){return new Compl(a.Re+b,a.Im);}
    public static Compl operator+(double a,Compl b){return new Compl(a+b.Re,b.Im);}
    public static Compl operator-(Compl a,double b){return new Compl(a.Re-b,a.Im);}
    public static Compl operator-(double a,Compl b){return new Compl(a-b.Re,-b.Im);}
    public static Compl Exp(Compl x){return new Compl(Math.Exp(x.Re)*Math.Cos(x.Im),Math.Exp(x.Re)*Math.Sin(x.Im));}
    public static double Abs(Compl x){return Math.Sqrt(x.Re*x.Re+x.Im*x.Im);}
}

class TestKernel
{
    static Compl ci=new Compl(0,1);
    static double lambda=1.0;

    static double J0(double x){
        double sum=0,s=-1,k2=1,xS=1,x2=x*x/4,sign=-1;int k=0;
        while(Math.Abs(s)>1e-12&&k<10000){sum+=s;k++;sign=-sign;k2/=k*k;xS*=x2;s=sign*k2*xS;}
        return -sum;
    }
    static double _Y0(double x){
        double sum=0,s=-1,k2=1,xS=1,x2=x*x/4,sign=-1,psi=-0.57721566+1;int k=0;
        while(Math.Abs(s)>1e-12&&k<10000){sum+=s;k++;sign=-sign;k2/=k*k;xS*=x2;psi+=1.0/k;s=sign*k2*xS*psi;}
        return sum;
    }
    static double N0(double x){return 2.0/Math.PI*(J0(x)*Math.Log(x/2)+_Y0(x));}
    static Compl H0_2(double x){return new Compl(J0(x),-N0(x));}

    // Ядро r(t,x) из кода
    static Compl r_code(double t,double x){
        double k=2*Math.PI/lambda;
        Compl s=new Compl(J0(k*Math.Abs(t-x)));
        if(Math.Abs(t-x)>1e-8)
            s=Math.PI*ci/2*s+(s-1.0)*Math.Log(k*Math.Abs(t-x)/2.0);
        else
            s=Math.PI*ci/2.0*s;
        s=s+Math.Log(k/2.0)+_Y0(k*Math.Abs(t-x));
        return -s;
    }

    // Что ядро ДОЛЖНО быть: -2*pi*(i/4)*H0(2)(k|t-x|) + ln|t-x|
    // (регуляризованное ядро = полное ядро минус логарифмическая сингулярность)
    static Compl r_expected(double t,double x){
        double k=2*Math.PI/lambda;
        double d=Math.Abs(t-x);
        if(d<1e-10) d=1e-10;
        Compl h=H0_2(k*d);
        return -Math.PI*ci/2.0*h + Math.Log(d);
    }

    // Полное ядро (без регуляризации): -2*pi*(i/4)*H0(2)(k|t-x|)
    static Compl r_full(double t,double x){
        double k=2*Math.PI/lambda;
        double d=Math.Abs(t-x);
        if(d<1e-10) d=1e-10;
        return -Math.PI*ci/2.0*H0_2(k*d);
    }

    static void Main(){
        Console.WriteLine("=== KERNEL ANALYSIS ===");
        Console.WriteLine();

        // Тест 1: Сравним r_code с r_expected и r_full для разных |t-x|
        Console.WriteLine("Compare r_code(t,x) with expected forms:");
        Console.WriteLine("{0,-10} {1,-30} {2,-30} {3,-30}","d=|t-x|","r_code","r_expected(+ln)","r_full(-pi*i/2*H)");
        double[] dists={0.01, 0.05, 0.1, 0.3, 0.5, 1.0, 1.5};
        foreach(double d in dists){
            Compl rc=r_code(0,d);
            Compl re=r_expected(0,d);
            Compl rf=r_full(0,d);
            Console.WriteLine("{0,-10:F3} {1,14:F6}+{2:F6}i  {3,14:F6}+{4:F6}i  {5,14:F6}+{6:F6}i",
                d,rc.Re,rc.Im,re.Re,re.Im,rf.Re,rf.Im);
        }
        Console.WriteLine();

        // Тест 2: Проверим r_code(t,x) - r_full(t,x) = ? (должно быть ln|t-x| если r_code = r_full + ln|t-x|)
        Console.WriteLine("r_code - r_full (should be ln|d| if regularization correct):");
        Console.WriteLine("{0,-10} {1,-20} {2,-20}","d","r_code-r_full","ln(d)");
        foreach(double d in dists){
            if(d<1e-10) continue;
            Compl diff=r_code(0,d)-r_full(0,d);
            Console.WriteLine("{0,-10:F3} {1,10:F6}+{2:F6}i  {3,10:F6}",d,diff.Re,diff.Im,Math.Log(d));
        }
        Console.WriteLine();

        // Тест 3: Проверим что int r(t,x)*1 dt ~ f(x) для phi(t)=const, u0=exp(...)
        // При u=0 на ленте: (i/4)*int phi*H0 dt = -u0
        // => int phi * (-pi*i/2*H0) dt = 2*pi*u0
        // => int phi * r_full dt = 2*pi*u0
        // С регуляризацией: int phi * r_code dt = int phi * r_full dt + int phi*ln|t-x| dt
        Console.WriteLine("Integral test: sum_n r(t_n, x_m) * (pi/M) for fixed x_m=0:");
        int M=20;
        double a=-1,b_=1;
        double[] xp=new double[M];
        for(int m=0;m<M;m++) xp[m]=(b_-a)/2.0*Math.Cos((2*m+1)/2.0/M*Math.PI)+(b_+a)/2.0;

        // Для x_m = 0
        Compl sum_code=new Compl(0);
        Compl sum_full=new Compl(0);
        for(int n=0;n<M;n++){
            sum_code=sum_code+r_code(xp[n],0)*(Math.PI/M);
            sum_full=sum_full+r_full(xp[n],0)*(Math.PI/M);
        }
        Console.WriteLine("  sum r_code(t_n, 0)*pi/M = {0:F6}+{1:F6}i",sum_code.Re,sum_code.Im);
        Console.WriteLine("  sum r_full(t_n, 0)*pi/M = {0:F6}+{1:F6}i",sum_full.Re,sum_full.Im);
        Console.WriteLine("  Expected f(0)/phi = -2*pi*1 = {0:F6}",(-2*Math.PI));
        Console.WriteLine();

        // Тест 4: Прямая проверка. Если phi(t)=c*u0(t,0), то
        // int (-pi*i/2*H0(2))(k|t-x|) * phi(t) dt = 2*pi*u0(x,0)
        // Решение СЛАУ должно давать phi такое, что подстановка в интеграл дает -u0 на поверхности
        Console.WriteLine("=== Direct substitution check ===");
        Console.WriteLine("Solve for N=5, theta=45, then check integral equation residual");

        double teta=Math.PI/4;
        int N_=5;
        Compl[] y_=new Compl[N_];

        // Solve
        var A=new Compl[N_,N_]; var B=new Compl[N_];
        for(int k=0;k<N_;k++){
            for(int j=0;j<N_;j++){
                var s=new Compl(0);
                for(int m=0;m<M;m++) for(int n=0;n<M;n++){
                    double tn=xp[n],xm=xp[m];
                    s=s+r_code(tn,xm)*Cheb_AB(j,tn,a,b_)*Cheb_AB(k,xm,a,b_);
                }
                s=s*(Math.PI*Math.PI/M/M);
                A[k,j]=s;
            }
            var sb=new Compl(0);
            for(int m=0;m<M;m++){
                Compl u0v=Compl.Exp(2*Math.PI/lambda*Math.Cos(teta)*ci*xp[m]);
                sb=sb+(-2*Math.PI*u0v)*Cheb_AB(k,xp[m],a,b_);
            }
            B[k]=sb*(Math.PI/M);
            Compl reg;if(k==0)reg=new Compl(Math.PI*Math.PI*Math.Log(b_-a),0);else reg=new Compl(Math.PI*Math.PI/2.0/(k+1),0);
            A[k,k]=A[k,k]+reg;
        }
        Gauss(A,B,y_,N_);

        Console.WriteLine("Coefficients:");
        for(int i=0;i<N_;i++) Console.WriteLine("  y[{0}]={1:F6}+{2:F6}i",i,y_[i].Re,y_[i].Im);

        // Reconstruct phi(t) = sum y_j*T_j(t)
        // Compute (i/4)*int phi(t)*H0(2)(k|t-x|) dt + u0(x,0) at test points
        Console.WriteLine("\nField u(x,0) = u0 + (i/4)*int phi*H0 dt:");
        double k_=2*Math.PI/lambda;
        for(int p=0;p<5;p++){
            double xtest=a+p*(b_-a)/4.0;
            Compl u0test=Compl.Exp(k_*Math.Cos(teta)*ci*xtest);
            Compl integral=new Compl(0);
            for(int m=0;m<M;m++){
                double t=xp[m];
                Compl phi=new Compl(0);
                for(int j=0;j<N_;j++) phi=phi+y_[j]*Cheb_AB(j,t,a,b_);
                double dist=Math.Abs(t-xtest);
                if(dist<1e-10)dist=1e-10;
                integral=integral+phi*H0_2(k_*dist)*(Math.PI/M);
            }
            Compl u_total=u0test+ci/4.0*integral;
            Console.WriteLine("  x={0:F2}: |u|={1:E3}, u0={2:F4}+{3:F4}i, u={4:F4}+{5:F4}i",
                xtest,Compl.Abs(u_total),u0test.Re,u0test.Im,u_total.Re,u_total.Im);
        }

        // Also check: sum A_kj*y_j == B_k?
        Console.WriteLine("\nResidual Ay-B (should be ~0):");
        // Need to re-form A and B
        var A2=new Compl[N_,N_]; var B2=new Compl[N_];
        for(int k=0;k<N_;k++){
            for(int j=0;j<N_;j++){
                var s=new Compl(0);
                for(int m=0;m<M;m++) for(int n=0;n<M;n++)
                    s=s+r_code(xp[n],xp[m])*Cheb_AB(j,xp[n],a,b_)*Cheb_AB(k,xp[m],a,b_);
                s=s*(Math.PI*Math.PI/M/M);
                A2[k,j]=s;
            }
            var sb=new Compl(0);
            for(int m=0;m<M;m++){
                Compl u0v=Compl.Exp(k_*Math.Cos(teta)*ci*xp[m]);
                sb=sb+(-2*Math.PI*u0v)*Cheb_AB(k,xp[m],a,b_);
            }
            B2[k]=sb*(Math.PI/M);
            Compl reg;if(k==0)reg=new Compl(Math.PI*Math.PI*Math.Log(b_-a),0);else reg=new Compl(Math.PI*Math.PI/2.0/(k+1),0);
            A2[k,k]=A2[k,k]+reg;
        }
        for(int k=0;k<N_;k++){
            Compl s=new Compl(0);
            for(int j=0;j<N_;j++) s=s+A2[k,j]*y_[j];
            Compl res=s-B2[k];
            Console.WriteLine("  k={0}: |Ay-B|={1:E3}",k,Compl.Abs(res));
        }
    }

    static double Cheb(int n,double x){
        if(n==0)return 1;if(n==1)return x;
        double T0=1,T1=x,T=0;for(int i=2;i<=n;i++){T=2*x*T1-T0;T0=T1;T1=T;}return T;
    }
    static double Cheb_AB(int n,double x,double a,double b){return Cheb(n,2.0/(b-a)*x-(b+a)/(b-a));}

    static void Gauss(Compl[,]A,Compl[]b,Compl[]x,int N){
        for(int i=0;i<N-1;i++){
            double max=Compl.Abs(A[i,i]);int maxN=i;
            for(int k=i+1;k<N;k++){double ss=Compl.Abs(A[k,i]);if(ss>max){max=ss;maxN=k;}}
            if(maxN!=i){for(int k=0;k<N;k++){var t=A[i,k];A[i,k]=A[maxN,k];A[maxN,k]=t;}var t2=b[i];b[i]=b[maxN];b[maxN]=t2;}
            Compl s=new Compl(1,0)/A[i,i];
            for(int j=i+1;j<N;j++){var s1=A[j,i]*s;for(int k=i+1;k<N;k++)A[j,k]=A[j,k]-s1*A[i,k];b[j]=b[j]-b[i]*s1;}
        }
        x[N-1]=b[N-1]/A[N-1,N-1];
        for(int i=N-2;i>=0;i--){var s=b[i];for(int j=N-1;j>i;j--)s=s-A[i,j]*x[j];x[i]=s/A[i,i];}
    }
}
