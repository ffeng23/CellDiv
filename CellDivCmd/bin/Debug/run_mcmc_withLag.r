###########rcode for ODE + Lag model

alpha=2.0;
gamma=0.6;
C0<-1;
t<-(0:100);

pdfOL<-exp(-1*alpha/gamma*(C0*exp(gamma*t)-gamma*t))*alpha*(C0*exp(gamma*t)-1);

cdfOL<-1-exp(-1*alpha/gamma*(C0*exp(gamma*t)-gamma*t));