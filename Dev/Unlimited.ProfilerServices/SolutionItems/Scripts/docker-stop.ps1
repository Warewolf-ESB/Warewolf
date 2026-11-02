docker stop $(docker ps --format '{{.Names}}' | findstr /i "profiler")
docker rm $(docker ps -a --format '{{.Names}}' | findstr /i "profiler")