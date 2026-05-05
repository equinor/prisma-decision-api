import os
from typing import List
from pydantic import Field
from dotenv import load_dotenv
from pydantic_settings import BaseSettings


class Config(BaseSettings):
    load_dotenv()

    ORIGINS: List[str] = [
        "http://localhost:5004",
        "https://frontend-prisma-decision-web-dev.radix.equinor.com",
        "https://frontend-prisma-decision-web-test.radix.equinor.com",
        "https://frontend-prisma-decision-web-prod.radix.equinor.com",
    ]

    APPINSIGHTS_CONNECTIONSTRING: str = Field(default=os.getenv("APPINSIGHTS_CONNECTIONSTRING", ""))
    APP_ENV: str = Field(default=os.getenv("APP_ENV", "local"))
    POOL_SIZE: int = 10
    MAX_OVERFLOW: int = 20
    POOL_RECYCLE: int = 1800
    # Database token duration in seconds (default: 50 minutes)
    DB_TOKEN_DURATION: int = 3000
    DEBUG: bool = False

    # Cache for 60 minutes
    CACHE_DURATION: int = 3600
    CACHE_MAX_SIZE: int = 256

    # use to enable PyInstrumentMiddleWare
    # this will generate a profile.html at repository root
    PROFILE: bool = False
    LOGGER: bool = True


config = Config()
