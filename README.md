# UrlShortener

URL shortcut API set with the following main features:

- Create a new URL shortcut given the original version
- Retrieve (redirect) to original url given shortened url
- Stateless API (for easy scalability)
- Usage of dependency injection pattern

Under the hood, the API uses a Sha 256 Hash + Base 62 encoding scheme to produce URLs that are 6 characters long.
This can produce 62^6 unique shorctuts or 56,800,235,584 (approx 57 Billion).
The URL generation logic has a collision detection retry logic, for the (unlikely) case that 2 urls map to the same shortcut, to create a robust shortcut generation URL.

The backend uses Azure Cosmos DB as the storage layer.  Cosmos DB has the following advantages:
- Scalable
- Geo-redundant
- Cost-effective
- Fast retrieval given partition key
- Extensible schema
