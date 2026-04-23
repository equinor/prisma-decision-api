from typing import Any
from fastapi import HTTPException
from httpx import AsyncClient
import httpx
import re
from src.dtos.user_dtos import UserIncomingDto
from src.config import config
from async_lru import alru_cache


def sanitize_graph_user_search(search: str) -> str:
    """Sanitize user search input by removing unsafe characters and limiting length."""
    sanitized = re.sub(r"[^A-Za-z0-9 .,'-]", "", search).strip()
    return re.sub(r"\s+", " ", sanitized)[:100]


async def _get_obo_access_token(client: AsyncClient, token: str) -> str:
    """Exchange user token for Graph API access token via On-Behalf-Of flow."""
    obo_response: httpx.Response = await client.post(
        f"https://login.microsoftonline.com/{config.TENANT_ID}/oauth2/v2.0/token",
        data={
            "grant_type": "urn:ietf:params:oauth:grant-type:jwt-bearer",
            "client_id": config.CLIENT_ID,
            "client_secret": config.CLIENT_SECRET,
            "assertion": token,
            "scope": "https://graph.microsoft.com/user.read",
            "requested_token_use": "on_behalf_of",
        },
    )

    if not obo_response.is_success:
        raise HTTPException(
            status_code=401,
            detail="Failed to obtain access token from Microsoft Graph API",
        )

    return obo_response.json()["access_token"]


@alru_cache(maxsize=config.CACHE_MAX_SIZE, ttl=config.CACHE_DURATION)
async def call_ms_graph_api(token: str) -> UserIncomingDto:
    """Fetch current user info from Microsoft Graph API /me endpoint."""
    async with AsyncClient() as client:
        try:
            access_token = await _get_obo_access_token(client, token)
            graph_response: httpx.Response = await client.get(
                "https://graph.microsoft.com/v1.0/me",
                headers={"Authorization": f"Bearer {access_token}"},
            )
            graph = graph_response.json()
            return UserIncomingDto(
                user_id=None,
                name=graph.get("displayName"),
                azure_id=graph.get("id"),
            )
        except HTTPException:
            raise
        except Exception as e:
            raise HTTPException(
                status_code=500,
                detail=f"Unexpected error in call_ms_graph_api: {str(e)}",
            )


@alru_cache(maxsize=config.CACHE_MAX_SIZE, ttl=config.CACHE_DURATION)
async def call_ms_graph_api_users(token: str, username: str) -> list[UserIncomingDto]:
    """Search for users in Microsoft Graph API by display name."""
    sanitized_username = sanitize_graph_user_search(username)
    if not sanitized_username:
        return []

    async with AsyncClient() as client:
        try:
            access_token = await _get_obo_access_token(client, token)
            graphApiEndpoint = f'https://graph.microsoft.com/v1.0/users?$search="displayName:{sanitized_username}"&$top=100'
            all_users: list[dict[str, Any]] = []

            graph_response: httpx.Response = await client.get(
                graphApiEndpoint,
                headers={
                    "Authorization": f"Bearer {access_token}",
                    "ConsistencyLevel": "eventual",
                },
                timeout=30.0,
            )

            if not graph_response.is_success:
                raise HTTPException(
                    status_code=graph_response.status_code,
                    detail=f"Graph API error: {graph_response.text}",
                )

            graph: dict[str, Any] = graph_response.json()
            all_users.extend(graph.get("value", []))

            filtered_users = [
                user
                for user in all_users
                if not (
                    "AZ" in user.get("displayName", "")
                    or any(char.isdigit() for char in user.get("displayName", ""))
                )
            ]

            return [
                UserIncomingDto(
                    user_id=None,
                    name=user.get("displayName") or "",
                    azure_id=user.get("id") or "",
                )
                for user in filtered_users
            ]
        except HTTPException:
            raise
        except Exception as e:
            raise HTTPException(
                status_code=500,
                detail=f"Unexpected error in call_ms_graph_api_users: {str(e)}",
            )
