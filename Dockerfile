#	Use a Microsoft image with .NET core runtime (https://hub.docker.com/r/microsoft/dotnet/tags/)
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS final

#	Set the working directory to /work
WORKDIR /work

#	Copy package
COPY PasWebApi/app .

# Make port 80 available to the world outside this container

EXPOSE 80

#	Define environment variables
ENV TODO ""

#	Run console app
CMD ["dotnet", "PasWebApi.dll"]