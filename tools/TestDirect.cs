using System;

class Compl
{
    public double Re,Im;
    public Compl(){Re=0;Im=0;}
    public Compl(double x){Re=x;Im=0;}
    public Compl(double x,double y){Re=x;Im=y;}
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
    public Compl Conj(){return new Compl(Re,-Im);}
}

class TestDirect
{
    static Compl ci=new Compl(0,1);
    static double lambda=1.0;
    static double a=-1,b_=1;

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
    static double Cheb(int n,double x){
        if(n==0)return 1;if(n==1)return x;double T0=1,T1=x,T=0;
        for(int i=2;i<=n;i++){T=2*x*T1-T0;T0=T1;T1=T;}return T;
    }
    static double ChebAB(int n,double x){return Cheb(n,2.0/(b_-a)*x-(b_+a)/(b_-a));}

    static Compl u0(double x,double z,double teta){
        double k=2*Math.PI/lambda;
        return Compl.Exp(k*Math.Cos(teta)*ci*x+k*Math.Sin(teta)*ci*z);
    }

    // Метод прямой коллокации: выбираем N точек коллокации на ленте
    // В каждой точке: u0(x_k) + (i/4)*int phi(t)*H0(2)(k|x_k-t|) dt = 0
    // phi(t) = sum y_j*T_j(t), интеграл берётся квадратурой по M узлам
    // Получаем: sum_j [sum_m (i/4)*H0(2)(k|x_k-t_m|)*T_j(t_m)*pi/M] * y_j = -u0(x_k)
    static void SolveCollocation(int N,int M,double teta,out Compl[]y,out double bcErr){
        double k=2*Math.PI/lambda;

        // Узлы квадратуры (Чебышев-Гаусс)
        double[] tp=new double[M];
        for(int m=0;m<M;m++) tp[m]=(b_-a)/2.0*Math.Cos((2*m+1)/2.0/M*Math.PI)+(b_+a)/2.0;

        // Точки коллокации (равномерные внутри ленты)
        double[] xc=new double[N];
        for(int i=0;i<N;i++) xc[i]=a+(i+0.5)*(b_-a)/N;

        var A=new Compl[N,N];
        var B=new Compl[N];

        for(int ik=0;ik<N;ik++){
            for(int j=0;j<N;j++){
                Compl s=new Compl(0);
                for(int m=0;m<M;m++){
                    double dist=Math.Abs(xc[ik]-tp[m]);
                    if(dist<1e-10)dist=1e-10;
                    s=s+H0_2(k*dist)*ChebAB(j,tp[m]);
                }
                A[ik,j]=s*ci/4.0*(Math.PI/M);
            }
            B[ik]=-u0(xc[ik],0,teta);
        }

        y=new Compl[N];
        Gauss(A,B,y,N);

        // Verify BC at different points
        int Mtest=40;
        double dx=(b_-a)/Mtest;
        double sumErr=0;
        for(int i=0;i<=Mtest;i++){
            double x=a+i*dx;
            Compl uval=u0(x,0,teta);
            for(int j=0;j<N;j++){
                Compl intj=new Compl(0);
                for(int m=0;m<M;m++){
                    double dist=Math.Sqrt((x-tp[m])*(x-tp[m]));
                    if(dist<1e-10)dist=1e-10;
                    intj=intj+H0_2(k*dist)*ChebAB(j,tp[m]);
                }
                uval=uval+y[j]*intj*ci/4.0*(Math.PI/M);
            }
            sumErr+=Compl.Abs(uval)/Math.Max(Compl.Abs(u0(x,0,teta)),0.1);
        }
        bcErr=sumErr/(Mtest+1);
    }

    // Метод Галеркина (текущий, но с полным ядром без регуляризации)
    static void SolveGalerkinFull(int N,int M,double teta,out Compl[]y,out double bcErr){
        double k=2*Math.PI/lambda;
        double[] xp=new double[M];
        for(int m=0;m<M;m++) xp[m]=(b_-a)/2.0*Math.Cos((2*m+1)/2.0/M*Math.PI)+(b_+a)/2.0;

        var A=new Compl[N,N];
        var B=new Compl[N];

        for(int ik=0;ik<N;ik++){
            for(int j=0;j<N;j++){
                Compl s=new Compl(0);
                for(int m=0;m<M;m++) for(int n=0;n<M;n++){
                    double dist=Math.Abs(xp[n]-xp[m]);
                    if(dist<1e-10)dist=1e-10;
                    Compl kernel=ci/4.0*H0_2(k*dist);
                    s=s+kernel*ChebAB(j,xp[n])*ChebAB(ik,xp[m]);
                }
                A[ik,j]=s*(Math.PI*Math.PI/M/M);
            }
            Compl sb=new Compl(0);
            for(int m=0;m<M;m++) sb=sb+(-u0(xp[m],0,teta))*ChebAB(ik,xp[m]);
            B[ik]=sb*(Math.PI/M);
        }

        y=new Compl[N];
        Gauss(A,B,y,N);

        // Verify BC
        int Mtest=40;
        double dx=(b_-a)/Mtest;
        double sumErr=0;
        for(int i=0;i<=Mtest;i++){
            double x=a+i*dx;
            Compl uval=u0(x,0,teta);
            for(int j=0;j<N;j++){
                Compl intj=new Compl(0);
                for(int m=0;m<M;m++){
                    double dist=Math.Abs(x-xp[m]);
                    if(dist<1e-10)dist=1e-10;
                    intj=intj+H0_2(k*dist)*ChebAB(j,xp[m]);
                }
                uval=uval+y[j]*intj*ci/4.0*(Math.PI/M);
            }
            sumErr+=Compl.Abs(uval)/Math.Max(Compl.Abs(u0(x,0,teta)),0.1);
        }
        bcErr=sumErr/(Mtest+1);
    }

    static void Gauss(Compl[,]A,Compl[]b,Compl[]x,int N){
        for(int i=0;i<N-1;i++){
            double max=Compl.Abs(A[i,i]);int maxN=i;
            for(int k=i+1;k<N;k++){double ss=Compl.Abs(A[k,i]);if(ss>max){max=ss;maxN=k;}}
            if(maxN!=i){for(int k=0;k<N;k++){var t=A[i,k];A[i,k]=A[maxN,k];A[maxN,k]=t;}var t2=b[i];b[i]=b[maxN];b[maxN]=t2;}
            if(Compl.Abs(A[i,i])<1e-14){Console.WriteLine("Singular at i="+i);return;}
            Compl s=new Compl(1,0)/A[i,i];
            for(int j=i+1;j<N;j++){var s1=A[j,i]*s;for(int k=i+1;k<N;k++)A[j,k]=A[j,k]-s1*A[i,k];b[j]=b[j]-b[i]*s1;}
        }
        if(Compl.Abs(A[N-1,N-1])<1e-14){Console.WriteLine("Singular at last");return;}
        x[N-1]=b[N-1]/A[N-1,N-1];
        for(int i=N-2;i>=0;i--){var s=b[i];for(int j=N-1;j>i;j--)s=s-A[i,j]*x[j];x[i]=s/A[i,i];}
    }

    static void Main(){
        Console.WriteLine("=== COMPARING METHODS ===");
        Console.WriteLine();

        double[] angles={30,45,60,80};
        int[] Ns={5,8,10};
        int M=30;

        Console.WriteLine("METHOD 1: Direct Collocation (uniform points, full H0 kernel)");
        Console.WriteLine("{0,-8} {1,-5} {2,-12}","Angle","N","BC_error%");
        Console.WriteLine(new string('-',30));
        foreach(double ang in angles){
            double teta=ang/180.0*Math.PI;
            foreach(int n in Ns){
                Compl[] y;double bcErr;
                SolveCollocation(n,M,teta,out y,out bcErr);
                Console.WriteLine("{0,-8:F0} {1,-5} {2,-12:P2}",ang,n,bcErr);
            }
        }

        Console.WriteLine();
        Console.WriteLine("METHOD 2: Galerkin with full H0 kernel (no regularization)");
        Console.WriteLine("{0,-8} {1,-5} {2,-12}","Angle","N","BC_error%");
        Console.WriteLine(new string('-',30));
        foreach(double ang in angles){
            double teta=ang/180.0*Math.PI;
            foreach(int n in Ns){
                Compl[] y;double bcErr;
                SolveGalerkinFull(n,M,teta,out y,out bcErr);
                Console.WriteLine("{0,-8:F0} {1,-5} {2,-12:P2}",ang,n,bcErr);
            }
        }
    }
}
