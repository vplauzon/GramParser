#	Build docker container
sudo docker build -t vplauzon/pas-web-api .

#	Publish image
#sudo docker push vplauzon/pas-web-api

#	Test image
#sudo docker run vplauzon/pas-web-api -p 4000:80
#curl localhost:4000
#sudo docker run -it vplauzon/pas-web-api -p 4000:80 bash
