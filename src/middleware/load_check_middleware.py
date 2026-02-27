from fastapi import Request
from fastapi.responses import JSONResponse
from src.logger import get_dot_api_logger
from starlette.middleware.base import BaseHTTPMiddleware
import psutil

logger = get_dot_api_logger()

class LoadCheckMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):  # type: ignore
        ram_percent: float = psutil.virtual_memory().percent
        cpu_percent: float = psutil.cpu_percent()

        if cpu_percent > 85 or ram_percent > 85:
            logger.error(f"Request rejected due to heavy load. RAM: {ram_percent}%, CPU: {cpu_percent}%")
            return JSONResponse(
                status_code=503, content={"message": "Server is currently under heavy load, try again later."}
            )
        response = await call_next(request)
        return response