# Graph Index

Busca por concepto:

```dataview
TABLE WITHOUT ID
file.link AS Method, file.etags AS Tags
FROM "."
WHERE contains(file.name, "Add")
LIMIT 20
```

## Por funcionalidad

| Busca | Query |
|-------|-------|
| Controllers | `contains(file.name, "Controller")` |
| Repositories | `contains(file.name, "Repository")` |
| Handlers | `contains(file.name, "Handler")` |
| Kafka | `contains(file.name, "Consumer")` |
| gRPC | `contains(file.name, "Grpc")` |
| Services | `contains(file.name, "Service")` |
| Entities | `contains(file.name, ".cs") AND !contains(file.name, "Handler") AND !contains(file.name, "Controller")` |

## God Nodes
- [[UsersController]] (21 conexiones)
- [[AuthController]] (20 conexiones)
- [[DashboardController]] (19 conexiones)
- [[ReservasController]] (15 conexiones)