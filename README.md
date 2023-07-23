# SQL Server Monitoring
This is ready to use solution consisting of Prometheus metrics collector and Grafana with dashboards to monitor key metrics in MS SQL Server.
This solution was born to fill absence of free monitoring solutions for SQL server.

## Installation

Clone this repository to any server with Docker installed. Goto folder `config` and make copy of `sql-collector.example.yaml` to `sql-collector.yaml`.
Edit SQL server connection string and point it to your SQL server.
Then just run containers:
```sh
docker compose up -d
```

Open your browser with URL http://localhost:3000/. Use 'admin' and 'monitoring' to login into Grafana.
Prometheus UI is available at http://localhost:9090/