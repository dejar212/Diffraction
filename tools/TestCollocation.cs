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

class TestCollocation
{
    static Compl ci=new Compl(0,1);
    static double lambda=1.0, a=-1, b_=1;

    static double J0(double x){double sum=0,s=-1,k2=1,xS=1,x2=x*x/4,sign=-1;int k=0;while(Math.Abs(s)>1e-12&&k<10000){sum+=s;k++;sign=-sign;k2/=k*k;xS*=x2;s=sign*k2*xS;}return -sum;}
    static double _Y0(double x){double sum=0,s=-1,k2=1,xS=1,x2=x*x/4,sign=-1,psi=-0.57721566+1;int k=0;while(Math.Abs(s)>1e-12&&k<10000){sum+=s;k++;sign=-sign;k2/=k*k;xS*=x2;psi+=1.0/k;s=sign*k2*xS*psi;}return sum;}
    static double N0(double x){return 2.0/Math.PI*(J0(x)*Math.Log(x/2)+_Y0(x));}
    static Compl H0_2(double x){return new Compl(J0(x),-N0(x));}
    static double Cheb(int n,double x){if(n==0)return 1;if(n==1)return x;double T0=1,T1=x,T=0;for(int i=2;i<=n;i++){T=2*x*T1-T0;T0=T1;T1=T;}return T;}
    static double ChebAB(int n,double x){return Cheb(n,2.0/(b_-a)*x-(b_+a)/(b_-a));}
    static Compl u0(double x,double z,double teta){double k=2*Math.PI/lambda;return Compl.Exp(k*Math.Cos(teta)*ci*x+k*Math.Sin(teta)*ci*z);}

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

    // Collocation with Chebyshev collocation points and Chebyshev quadrature
    static void SolveChebCollocation(int N,int M,double teta,out Compl[]y,out double bcErr){
        double k=2*Math.PI/lambda;
        double[] tp=new double[M];
        for(int m=0;m<M;m++) tp[m]=(b_-a)/2.0*Math.Cos((2*m+1)/2.0/M*Math.PI)+(b_+a)/2.0;

        // Chebyshev collocation points
        double[] xc=new double[N];
        for(int i=0;i<N;i++) xc[i]=(b_-a)/2.0*Math.Cos((2*i+1)/2.0/N*Math.PI)+(b_+a)/2.0;

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
        bcErr=ComputeBCError(y,N,M,teta);
    }

    // Collocation with Chebyshev points, higher-order quadrature (trapezoidal)
    static void SolveChebCollocationTrap(int N,int M,double teta,out Compl[]y,out double bcErr){
        double k=2*Math.PI/lambda;

        // Chebyshev collocation points
        double[] xc=new double[N];
        for(int i=0;i<N;i++) xc[i]=(b_-a)/2.0*Math.Cos((2*i+1)/2.0/N*Math.PI)+(b_+a)/2.0;

        var A=new Compl[N,N];
        var B=new Compl[N];
        double dt=(b_-a)/M;

        for(int ik=0;ik<N;ik++){
            for(int j=0;j<N;j++){
                Compl s=new Compl(0);
                for(int m=0;m<M;m++){
                    double t=a+(m+0.5)*dt;
                    double dist=Math.Abs(xc[ik]-t);
                    if(dist<1e-10)dist=1e-10;
                    s=s+H0_2(k*dist)*ChebAB(j,t)*dt;
                }
                A[ik,j]=s*ci/4.0;
            }
            B[ik]=-u0(xc[ik],0,teta);
        }
        y=new Compl[N];
        Gauss(A,B,y,N);
        bcErr=ComputeBCErrorTrap(y,N,M,teta);
    }

    static double ComputeBCError(Compl[]y,int N,int M,double teta){
        double k=2*Math.PI/lambda;
        double[] tp=new double[M];
        for(int m=0;m<M;m++) tp[m]=(b_-a)/2.0*Math.Cos((2*m+1)/2.0/M*Math.PI)+(b_+a)/2.0;
        int Mtest=40;double dx=(b_-a)/Mtest;double sumErr=0;
        for(int i=0;i<=Mtest;i++){
            double x=a+i*dx;
            Compl uval=u0(x,0,teta);
            for(int j=0;j<N;j++){
                Compl intj=new Compl(0);
                for(int m=0;m<M;m++){double dist=Math.Abs(x-tp[m]);if(dist<1e-10)dist=1e-10;intj=intj+H0_2(k*dist)*ChebAB(j,tp[m]);}
                uval=uval+y[j]*intj*ci/4.0*(Math.PI/M);
            }
            sumErr+=Compl.Abs(uval)/Math.Max(Compl.Abs(u0(x,0,teta)),0.1);
        }
        return sumErr/(Mtest+1);
    }

    static double ComputeBCErrorTrap(Compl[]y,int N,int M,double teta){
        double k=2*Math.PI/lambda;double dt=(b_-a)/M;
        int Mtest=40;double dx=(b_-a)/Mtest;double sumErr=0;
        for(int i=0;i<=Mtest;i++){
            double x=a+i*dx;
            Compl uval=u0(x,0,teta);
            for(int j=0;j<N;j++){
                Compl intj=new Compl(0);
                for(int m=0;m<M;m++){double t=a+(m+0.5)*dt;double dist=Math.Abs(x-t);if(dist<1e-10)dist=1e-10;intj=intj+H0_2(k*dist)*ChebAB(j,t)*dt;}
                uval=uval+y[j]*intj*ci/4.0;
            }
            sumErr+=Compl.Abs(uval)/Math.Max(Compl.Abs(u0(x,0,teta)),0.1);
        }
        return sumErr/(Mtest+1);
    }

    static void Main(){
        Console.WriteLine("=== COLLOCATION VARIANTS ===");
        double[] angles={10,30,45,60,80};

        Console.WriteLine("\nCHEB-COLLOCATION + Cheb-Gauss quad (M=40):");
        Console.WriteLine("{0,-8} {1,-5} {2,-12}","Angle","N","BC_error%");
        foreach(double ang in angles){
            double teta=ang/180.0*Math.PI;
            foreach(int n in new[]{5,8,10,12,15}){
                Compl[]y;double e;
                SolveChebCollocation(n,40,teta,out y,out e);
                Console.WriteLine("{0,-8:F0} {1,-5} {2,-12:P2}",ang,n,e);
            }
        }

        Console.WriteLine("\nCHEB-COLLOCATION + Trapezoidal quad (M=80):");
        Console.WriteLine("{0,-8} {1,-5} {2,-12}","Angle","N","BC_error%");
        foreach(double ang in angles){
            double teta=ang/180.0*Math.PI;
            foreach(int n in new[]{5,8,10,12,15}){
                Compl[]y;double e;
                SolveChebCollocationTrap(n,80,teta,out y,out e);
                Console.WriteLine("{0,-8:F0} {1,-5} {2,-12:P2}",ang,n,e);
            }
        }

        Console.WriteLine("\nCHEB-COLLOCATION + Trapezoidal quad (M=200):");
        Console.WriteLine("{0,-8} {1,-5} {2,-12}","Angle","N","BC_error%");
        foreach(double ang in angles){
            double teta=ang/180.0*Math.PI;
            foreach(int n in new[]{5,8,10}){
                Compl[]y;double e;
                SolveChebCollocationTrap(n,200,teta,out y,out e);
                Console.WriteLine("{0,-8:F0} {1,-5} {2,-12:P2}",ang,n,e);
            }
        }
    }
}
