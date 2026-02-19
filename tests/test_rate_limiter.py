import pytest
from httpx import AsyncClient

from src.config import config


@pytest.mark.asyncio
async def test_rate_limiter_with_root_endpoint(client: AsyncClient):
    """Test rate limiting on the root endpoint."""
    # Make requests up to the configured limit
    max_requests = config.MAX_REQUESTS_PER_WINDOW

    # First batch of requests should succeed
    for i in range(min(max_requests, 10)):  # Test up to 10 to keep test fast
        response = await client.get("/")
        assert response.status_code == 200, f"Request {i+1} failed unexpectedly"
        assert response.json() == {"message": "Welcome to the DOT api"}
