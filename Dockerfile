FROM ubuntu:eoan-20200207
WORKDIR /webapp
RUN  apt-get -y update && apt-get -y install wget\
&& wget -q https://packages.microsoft.com/config/ubuntu/19.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb&&\
dpkg -i packages-microsoft-prod.deb &&\
apt-get update &&\
apt-get install apt-transport-https &&\
apt-get update &&\
apt-get -y install  dotnet-sdk-2.1&&\
apt-get -y install  dotnet-sdk-3.1&&\
 apt-get -y install  aspnetcore-runtime-2.1&&\
 apt-get -y install  aspnetcore-runtime-3.1&&\
  dotnet new -i Amazon.Lambda.Templates &&\
apt-get update
