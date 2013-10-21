######r code for run MCMC for estimating the Discrete Normal distribution parameters########
##for variations among siblings
###########################################

#define variables
#div_dat<-read.table("division_destiny_withBCL-3and_cpg40hr.csv", header=T, sep=",")

#div_dat<-read.table("division_destiny_withBCL-3and_cpg60hr.csv", header=T, sep=",")

lamdaArr<-c();
sigmaArr<-c();
qArr<-c();
logllArr<-c();

lastLogll<- -1E+100;
lastLamda<-0;
lastQ<-0;
lastSigma<-0;

x<-c{-2,-1,0,1,2};
#y<-div_dat[c(1:8),2];

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
lamdaBa<-5.02;
qBa<-0.5118;

x<-c(1:8);
denormPredicted<-sum((lamdaBa^x)*qBa^(x*(x-1)/2))
normPredicted<-rep(0,length(x));
	i<-1;
	for(i in c(1:length( x)))
	{
		normPredicted[i]<-lamdaBa^x[i]*qBa^(x[i]*(x[i]-1)/2);
		normPredicted[i]<-normPredicted[i]/denormPredicted;
	}
	
	
	plot(c(0,9), c(0,0.35), col=1, type="n")
points(x, normPredicted, col=2)
points(x, y, col=3)



################best output so far for *figure9b* ##############
 