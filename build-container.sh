#	Build docker container
sudo docker build -t vplauzon/pas-web-api .

#	Publish image
#sudo docker push vplauzon/pas-web-api

#	Test image
#sudo docker run --name test-web-api -d -p 4000:80 vplauzon/pas-web-api:<build version>
#curl localhost:4000
#sudo docker run -it vplauzon/pas-web-api -p 4000:80 bash
#  Clean up after test
#sudo docker stop test-web-api && sudo docker container prune -f