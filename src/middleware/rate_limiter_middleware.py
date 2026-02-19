from fastapi import Request
from slowapi import Limiter
from slowapi.util import get_remote_address
from src.config import config


def get_client_ip(request: Request) -> str:
    """Extract client IP, considering X-Forwarded-For for proxied requests."""
    forwarded = request.headers.get("X-Forwarded-For")
    if forwarded:
        return forwarded.split(",")[0].strip()
    return get_remote_address(request) or "unknown"


# Default rate limit string (e.g., "100/minute")
DEFAULT_RATE_LIMIT = f"{config.MAX_REQUESTS_PER_WINDOW}/{config.RATE_LIMIT_WINDOW}second"
# Create limiter instance with client IP as the key
limiter = Limiter(key_func=get_client_ip, default_limits=[DEFAULT_RATE_LIMIT])
