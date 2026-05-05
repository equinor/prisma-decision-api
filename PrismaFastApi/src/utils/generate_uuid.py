import uuid


class GenerateUuid:
    @staticmethod
    def as_string(x: int | str) -> str:
        return str(uuid.uuid5(uuid.NAMESPACE_DNS, f"{x}"))

    @staticmethod
    def as_uuid(x: int | str) -> uuid.UUID:
        return uuid.uuid5(uuid.NAMESPACE_DNS, f"{x}")
