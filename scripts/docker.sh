#!/bin/bash

docker build -t idiordiev/online-shop-orders-service -f ./../src/Orders/OnlineShop.Orders.Api/Dockerfile ./..
docker build -t idiordiev/online-shop-orders-service-migrations -f ./src/Orders/OnlineShop.Orders.Migrations/Dockerfile ./..
docker push idiordiev/online-shop-orders-service
docker push idiordiev/online-shop-orders-service-migrations

docker build -t idiordiev/online-shop-users-service -f ./../src/Users/OnlineShop.Users.Api/Dockerfile ./..
docker build -t idiordiev/online-shop-users-service-migrations -f ./../src/Users/OnlineShop.Users.Migrations/Dockerfile ./..
docker push idiordiev/online-shop-users-service
docker push idiordiev/online-shop-users-service-migrations

docker build -t idiordiev/online-shop-store-service -f ./../src/Store/OnlineShop.Store.Api/Dockerfile ./..
docker build -t idiordiev/online-shop-store-service-migrations -f ./../src/Store/OnlineShop.Store.Migrations/Dockerfile ./..
docker push idiordiev/online-shop-store-service
docker push idiordiev/online-shop-store-service-migrations

docker build -t idiordiev/online-shop-baskets-service -f ./../src/Baskets/OnlineShop.Baskets.Api/Dockerfile ./..
docker build -t idiordiev/online-shop-baskets-service-migrations -f ./../src/Baskets/OnlineShop.Baskets.Migrations/Dockerfile ./..
docker push idiordiev/online-shop-baskets-service
docker push idiordiev/online-shop-baskets-service-migrations

docker build -t idiordiev/online-shop-email-service  -f ./../src/Email/OnlineShop.Email.Console/Dockerfile ./../
docker push idiordiev/online-shop-email-service

docker build -t idiordiev/online-shop-entity-history  -f ./../src/EntityHistory/OnlineShop.EntityHistory.Console/Dockerfile ./../
docker build -t idiordiev/online-shop-entity-history-migrations  -f ./../src/EntityHistory/OnlineShop.EntityHistory.Migrations/Dockerfile ./../
docker push idiordiev/online-shop-entity-history
docker push idiordiev/online-shop-entity-history-migrations
