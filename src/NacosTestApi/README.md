-- 啟動.Net 7專案
docker build -f NacosApi7/Dockerfile -t nacosapi7 .
docker run -f nacosapi7

-- 啟動.Net 8專案
docker build -f NacosApi8/Dockerfile -t nacosapi8 .
docker run -f nacosapi8