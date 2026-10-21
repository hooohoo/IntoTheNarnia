# Professor_House 천장 접합면 정리

천장의 높이, 외곽 형태 및 배치를 유지하면서 겹친 표면만 제거했습니다.

- Ceiling_1st는 원본 메시를 유지했습니다.
- Ceiling_1st (1), (2), (3)의 메시와 MeshCollider를 같은 경계로 잘랐습니다.
- 정리한 겹침: Ceiling_1st ↔ (1) 약 1.441㎡, Ceiling_1st ↔ (2) 약 24㎡, (1) ↔ (3) 약 5㎡. 합계 약 30.441㎡입니다.
- 천장 오브젝트 4개와 위치·회전·스케일을 유지했습니다. 기존 삼각형 격자를 바탕으로 자르고 UV와 노멀·탄젠트를 보간해 무늬 비율을 유지했습니다.
- 씬 전용 메시 3개는 Assets/Scenes/Professor_House/Meshes/ProfessorHouse_CeilingSurfaces.asset 한 파일에 보관했습니다.
- 재질과 기존 라이트맵 37장은 그대로 유지했습니다. 천장 Renderer의 기존 lightmapIndex는 -1이며, 라이팅을 다시 베이크하지 않았습니다.

## 검증

실제로 Unity에 적용된 메시로 검사했습니다. 0.01 mm의 좌표 오차 허용 범위에서 겹친 영역, 표면 누락, 외곽 확장, 뒤집힌 삼각형은 모두 0입니다. 천장 높이 변화는 0입니다.
기존 겹침 중심 3곳과 각 천장 내부 4곳에서 위쪽으로 쏜 Ray가 천장 Collider 한 개씩만 통과했습니다.
전후 화면에서 큰 겹침 구간의 사각형 얼룩이 제거된 것을 확인했습니다. Comparisons 폴더에서 확인할 수 있습니다.

scene-validation.json에는 환경 오브젝트 변형이 없고 천장 MeshFilter 3개만 교체된 결과가 기록되어 있습니다.
@UI는 ScreenSpaceOverlay Canvas이며, 화면 높이 613→614에 따라 화면 중심 Y가 306.5→307로 자동 계산된 차이가 있습니다. UI 설정이나 자식 배치는 수정하지 않았습니다.
geometry-validation.json에는 적용 메시의 표면 검사, uv-validation.json에는 UV 유지 검사가 기록되어 있습니다.

## 백업

Professor_House.unity와 .meta가 천장 작업 직전의 백업입니다. 앞서 완료한 러그·벽·웨인스코팅 정리 결과를 포함합니다.
ceilings-before.json, ceilings-after.json 및 ceiling-cleanup-plan.json에는 원본과 정리 후의 메시 및 연결 정보가 있습니다.

이 메시들은 현재 천장 배치를 기준으로 경계를 맞춘 것입니다. 향후 천장 위치나 크기를 바꾸면 접합 경계도 다시 맞춰야 합니다.
