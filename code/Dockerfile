#	Use a Microsoft image with .NET core runtime (https://hub.docker.com/r/microsoft/dotnet/tags/)
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final

#	Set the working directory to /work
WORKDIR /work

#	Copy package
COPY build-output/PasWebApi .

# Make port 80 available to the world outside this container
EXPOSE 80

#	Define environment variables
ENV TODO ""

#	Run console app
CMD ["dotnet", "GramParserWorkbench.dll"]