from fastapi import HTTPException, Request
from slowapi import Limiter
from slowapi.util import get_remote_address
from src.config import config


def get_client_key(request: Request) -> str:
    """
    Get rate limit key for the client.

    Priority:
    1. IP address from X-Forwarded-For header (for proxied requests)
    2. Direct client IP address(for local/development requests)
    """
    # Try X-Forwarded-For first (for requests )
    forwarded = request.headers.get("X-Forwarded-For")
    if forwarded:
        ip = forwarded.split(",")[0].strip()
        if ip:
            return f"ip:{ip}"

    # Try direct client IP for local
    ip = get_remote_address(request)
    if ip:
        return f"ip:{ip}"

    # Cannot identify client - reject request
    raise HTTPException(status_code=403, detail="Unable to identify client for rate limiting")


# Default rate limit string (e.g., "100/minute")
DEFAULT_RATE_LIMIT = f"{config.MAX_REQUESTS_PER_WINDOW}/{config.RATE_LIMIT_WINDOW}second"
# Create limiter instance with client key function
limiter = Limiter(key_func=get_client_key, default_limits=[DEFAULT_RATE_LIMIT])
