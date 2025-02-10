include .env

DATABASE_URL=postgres://$(DB_USERNAME):$(DB_PASSWORD)@$(DB_URL):$(DB_PORT)/$(DB_NAME)?sslmode=disable
MIGRATION_DIR=migrations

# Run Migrations
migrate-up:
	migrate -path $(MIGRATION_DIR) -database "$(DATABASE_URL)" up

migrate-down:
	migrate -path $(MIGRATION_DIR) -database "$(DATABASE_URL)" down

# Rollback the last migration
migrate-rollback:
	migrate -path $(MIGRATION_DIR) -database "$(DATABASE_URL)" down 1

# Create a new migration file
migrate-create:
	@if [ -n "$(name)" ]; then \
		migrate create -ext sql -dir $(MIGRATION_DIR) -seq $(name); \
	else \
		echo "Migration name required, usage: make migrate-create name=add_users_table"; \
	fi

# Clean migration history if needed (use with caution!)
migrate-drop:
	migrate -path $(MIGRATION_DIR) -database "$(DATABASE_URL)" drop

build:;
	cd client && bun run build
	rm -rf wwwroot/*
	cp -R client/dist/* wwwroot/

run:
	cd client && ng build
	rm -rf wwwroot/*
	cp -R client/dist/client/browser/* wwwroot/
	dotnet run

