from fastapi.security import OAuth2AuthorizationCodeBearer
from fastapi import status, Depends, HTTPException
import requests
from authlib.jose.errors import JoseError
from authlib.jose import JsonWebToken
import time
from src.config import config

oauth2_scheme = OAuth2AuthorizationCodeBearer(
    authorizationUrl=config.AUTH_URL,
    tokenUrl=config.TOKEN_URL,
    scopes={f"{config.SCOPE}": "Read"},
)


def get_jwks(jwks_uri: str):
    return requests.get(jwks_uri).json()


def get_allowed_audiences() -> list[str]:
    audience = (config.AUDIENCE or "").strip()
    if not audience:
        return []

    return [audience, f"api://{audience}"]


def get_claims_options() -> dict[str, dict[str, object]]:
    allowed_audiences = get_allowed_audiences()
    return {
        "iss": {"essential": True, "value": config.ISSUER},
        "aud": {"essential": True, "values": allowed_audiences},
        "exp": {"essential": True},
        "nbf": {"essential": True},
        "iat": {"essential": True},
    }


# @timed_lru_cache(seconds=config.CACHE_DURATION, maxsize=config.CACHE_MAX_SIZE)
def verify_token(token: str = Depends(oauth2_scheme)) -> str:
    try:
        claims_options = get_claims_options()
        jwt = JsonWebToken(["RS256"])  # Only allow RS256
        jwks = get_jwks(
            "https://login.microsoftonline.com/3aa4a235-b6e2-48d5-9195-7fcf05b459b0/discovery/v2.0/keys"
        )
        claims = jwt.decode(token, jwks, claims_options=claims_options)
        claims.validate(now=time.time(), leeway=1)
        return token
    except JoseError as e:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=f"Token validation failed: {str(e)}",
        )
