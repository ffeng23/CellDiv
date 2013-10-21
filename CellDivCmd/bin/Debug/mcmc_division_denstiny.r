######r code for run MCMC for estimating the Discrete Normal distribution parameters########
## see definition for normal distribution in Kemp, Characterizations of a discrete normal distribution
## Journal of statistical planning and inference 63(1997) p223
###########################################

#define variables
div_dat<-read.table("division_destiny_withBCL-3and_cpg40hr.csv", header=T, sep=",")

div_dat<-read.table("division_destiny_withBCL-3and_cpg60hr.csv", header=T, sep=",")

lamdaArr<-c();
sigmaArr<-c();
qArr<-c();
logllArr<-c();

lastLogll<- -1E+100;
lastLamda<-0;
lastQ<-0;
lastSigma<-0;

x<-div_dat[c(1:8),1];
y<-div_dat[c(1:8),2];

for( k in c(1:100000))
{
print(paste("loop:",k,sep=""));
	#parameters for discrete normal, draw next one
	lamda<-runif(1, 0.00,1000)
	q<-rbeta(1,1,1);
	sigma<-runif(1,0, 1)
	
	
	#calculate the likelihood of MCMC chain
	
	denorm<-sum((lamda^x)*q^(x*(x-1)/2))
	
	norm<-rep(0,length(x));
	i<-1;
	for(i in c(1:length( x)))
	{
		norm[i]<-lamda^x[i]*q^(x[i]*(x[i]-1)/2);
		norm[i]<-norm[i]/denorm;
	}
	logll<-0;
	##calculate the loglikelihood
	for(j in c(1:length(norm)))
	{
		logll<-logll+log(dnorm(y[j],norm[j],sigma));
	}
	
	flag<-FALSE;
	####compare the logll
	if(logll>=lastLogll) {
	#update everything
		flag<-TRUE;
	} else{
		u<-runif(1,0,1);
		if(log(u)< (logll-lastLogll)){
			flag<-TRUE;
		}
	}
	
	###check to upate
	if(flag){
		lamdaArr<-c(lamdaArr, lamda);
		qArr<-c(qArr,q);
		logllArr<-c(logllArr,logll);
		sigmaArr<-c(sigmaArr,sigma);
		
		lastLogll<-logll;
		lastLamda<-lamda;
		lastQ<-q;
		lastSigma<-sigma;	
	} else {
		lamdaArr<-c(lamdaArr, lastLamda);
		qArr<-c(qArr,lastQ);
		logllArr<-c(logllArr,lastLogll);
		sigmaArr<-c(sigmaArr,lastSigma);
		#lastLogll<-logll;
	}
}

###run some statistics
op<-par(mfrow = c(2, 2), # 2 x 2 pictures on one plot
          pty = "s")       # square plotting region,
                           # independent of device size
                           
plot(c(1:length(logllArr)), logllArr,  col=2, main="LogLL", type="l")
#mtext(paste("totle number of cells:", length(xt[,2]), ""));

plot(c(1:length(logllArr)), sigmaArr,  col=2, main="sigma", type="l")
#mtext(paste("totle number of cells:", length(xt[,2]), ""));

plot(c(1:length(logllArr)), lamdaArr,  col=2, main="lamda", type="l")
#mtext(paste("totle number of cells:", length(xt[,2]), ""));

plot(c(1:length(logllArr)), qArr,  col=2, main="q", type="l")
#mtext(paste("totle number of cells:", length(xt[,2]), ""));

par(op);


low<-50000;#length(lamdaArr)/2;
high<-100000;
lamdaBa<-median(lamdaArr[c(low:high)])
qBa<-median(qArr[c(low:high)])
sigmaBa<-mean(sigmaArr[c(low:high)])
logllBa<-mean(logllArr[c(low:high)])


####verify
lamdaBa<-4.448718;
qBa<-		0.688582;

x<-c(1:8);
#x<-c(-2:2);
denormPredicted<-sum((lamdaBa^x)*qBa^(x*(x-1)/2))
normPredicted<-rep(0,length(x));
	i<-1;
	for(i in c(1:length( x)))
	{
		normPredicted[i]<-lamdaBa^x[i]*qBa^(x[i]*(x[i]-1)/2);
		normPredicted[i]<-normPredicted[i]/denormPredicted;
	}
	
jpeg("divisionDestiny_Figure9d.jpg");
	plot(c(0,9), c(0,0.4), col=1, type="n", main="Dvision Destiny (Discrete Normal Distribution)", xlab="generation", ylab="proportion")
points(x, normPredicted, col=2, pch=2)
points(x, y, col=3, pch=16)
lines(x, normPredicted, col=2, lwd=2, lty=2);
legend(6,0.3, c("fitted", "exp data"),text.col=c(2,3), lty=c(1,0), col= c(2,3),cex=0.9, pch=c(2,16));

dev.off();
################best output so far for *figure9b* ##############
> sigmaBa
[1] 0.00937503
> logllBa
[1] 17.88932
> lamdaBa
[1] 1.556716
> qBa
[1] 0.688582
> normPredicted
[1] 0.2901771668 0.3110487205 0.2295880537 0.1166879037 0.0408373990
[6] 0.0098411532 0.0016330128 0.0001865902
> y
[1] 0.262020854 0.308018868 0.261554121 0.120754717 0.025223436 0.003291956
[7] 0.001648461 0.001891758

###############best output so far for figure9d############
 sigmaBa
[1] 0.0626555
> lamdaBa
[1] 4.448718
> y
[1] 0.08250000 0.09750000 0.15333333 0.24583333 0.20000000 0.11250000 0.05083333
[8] 0.04416667
> qBa
[1] 0.6584583
> normPredicted
[1] 0.03630444 0.10634642 0.20512298 0.26051578 0.21786227 0.11996603 0.04349735
[8] 0.01038474
> 
