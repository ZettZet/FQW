from datetime import datetime
from enum import Enum
from typing import Optional

from pydantic import BaseModel


class Mode(Enum):
    CubeMeter = '0'
    Tonne = '1'


class Well(BaseModel):
    id: Optional[str]
    name: Optional[str]
    weights: dict[str, float]

    def __init__(self, **kwargs):
        kwargs['id'] = kwargs['properties']['id']
        kwargs['name'] = kwargs['properties']['name']
        weight = {str(datetime.strptime(key, '%d.%m.%Y').date()):value for key, value in kwargs['properties']['weights'].items()}
        kwargs['weights'] = weight
        super().__init__(**kwargs)


class Region(BaseModel):
    id: str
    wells: list[Well]

    def __init__(self, **kwargs):
        kwargs['id'] = kwargs['properties']['id']
        kwargs['wells'] = kwargs['properties']['wells']['features']
        super().__init__(**kwargs)

class GeoInput(BaseModel):
    regions: list[Region]

    def __init__(self, **kwargs):
        kwargs['regions'] = kwargs['features']
        super().__init__(**kwargs)

