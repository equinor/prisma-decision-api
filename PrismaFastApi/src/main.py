import uvicorn
from fastapi import FastAPI, status
import src.routes.solver_routes as solver_routes
import src.routes.structure_routes as structure_routes
from src.config import config
from src.middleware.py_instrument_middle_ware import PyInstrumentMiddleWare
from fastapi.middleware.cors import CORSMiddleware
from azure.monitor.opentelemetry import configure_azure_monitor  # type: ignore
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor  # type: ignore

from src.middleware.exception_handling_middleware import ExceptionFilterMiddleware
from src.middleware.load_check_middleware import LoadCheckMiddleware
from src.logger import DOT_API_LOGGER_NAME, get_dot_api_logger

logger = get_dot_api_logger()


app = FastAPI(
    swagger_ui_parameters={"syntaxHighlight": False},
)

if config.LOGGER:
    try:
        configure_azure_monitor(
            logger_name=DOT_API_LOGGER_NAME, connection_string=config.APPINSIGHTS_CONNECTIONSTRING
        )
        FastAPIInstrumentor.instrument_app(app)
        logger.info("Successfully configured telemetry after starting application")
    except Exception as e:
        logger.info("Error occurred while configuring telemetry: %s", e)

# Adding CORS middleware to the FastAPI application
app.add_middleware(
    CORSMiddleware,
    allow_origins=config.ORIGINS,  # List of allowed origins
    allow_credentials=True,  # Allow credentials (cookies, authorization headers, etc.)
    allow_methods=["*"],  # Allow all HTTP methods
    allow_headers=["*"],  # Allow all HTTP headers
)
app.add_middleware(LoadCheckMiddleware)
app.add_middleware(ExceptionFilterMiddleware)

if config.PROFILE:
    # this will generate a profile.html at repository root when running any endpoint
    app.add_middleware(PyInstrumentMiddleWare)


@app.get("/", status_code=status.HTTP_200_OK)
async def root():
    return {"message": "Welcome to the DOT api"}


app.include_router(
    solver_routes.router,
)
app.include_router(structure_routes.router)

if __name__ == "__main__":
    uvicorn.run("src.main:app", port=8080)
