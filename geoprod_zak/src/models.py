from datetime import date
from pydantic import BaseModel
from sqlmodel import SQLModel, Field

__all__ = ('ZakWell', 'ZakWellCalculated', 'ZakWellValue', 'GeoInput')


class ZakWell(SQLModel, table=True):
    id: int = Field(primary_key=True)
    well: str
    well_num: str
    dt: date
    zak: float
    zak_cum: float

    __tablename__: str = "ZakWell"


class Well(BaseModel):
    id: str
    name: str
    weights: dict[str, float]


class Region(BaseModel):
    id: str
    wells: list[Well]


class GeoInput(BaseModel):
    regions: list[Region]


class ZakWellCalculated(BaseModel):
    region: str
    id: str
    name: str
    result: float
    result_uw: float


class ZakWellValue(BaseModel):
    well: str
    well_num: str
    zak: float
    dt: date
