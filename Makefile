up:
	docker-compose up --build -d --wait
	
up-local:
	docker-compose up rabbitmq --build -d --wait
	docker-compose up mysql --build -d --wait
	docker-compose up httpbin --build -d --wait

up-clean:
	docker-compose down
	docker-compose up --build -d --wait

down:
	docker-compose down

build:
	dotnet build EmpresaProyecto.slnx

test:
	dotnet test EmpresaProyecto.slnx

clean:
	dotnet clean EmpresaProyecto.slnx

connect-db:
	docker exec -it mysql mysql -u devuser -pdevpass

rabbitmq-message:
	docker exec -it rabbitmq rabbitmqctl list_queues	