# SQL Server Monitoring
This is ready to use solution consisting of Prometheus metrics collector and Grafana with dashboards to monitor key metrics in MS SQL Server.
This solution was born to fill absence of free monitoring solutions for SQL server.

## Installation

Clone this repository to any server with Docker installed. Make copy of `env.example` to `.env` and adjust SQL
connection string. Then just run containers
```sh
docker compose up -d
```

Open your browser with URL http://localhost:3000/. Use 'admin' and 'monitoring' to login into Grafana.
Prometheus UI is available at http://localhost:9090/