FROM microsoft/aspnetcore-build

MAINTAINER Dan Wahlin

ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_URLS=http://*:80

WORKDIR /var/www/app

CMD ["/bin/bash", "-c", "dotnet restore && dotnet watch run"]
