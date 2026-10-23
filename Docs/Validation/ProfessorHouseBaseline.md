# 교수의 집 제작 기준점 검증 — 2026-10-23

기존 교수의 집 v2 구현, 승인된 아트 참고자료, 확정된 층별 도면 03을 함께 보존하는 기준점이다. 작업 브랜치는 `chore/professor-house-approved-baseline`이며 작업 커밋 1개 → main의 `--no-ff` 병합 1개 → atomic push 1회로 통합한다.

## 포함 범위

- 현재 Unity v2 홀·계단·회랑, 두 퀘스트 문, FBX·Blender 원본과 생성 도구, 라이트맵·NavMesh·머티리얼·애니메이션.
- Lucy·교수의 집 v2 레퍼런스와 구역별 승인 보드·조사 자료.
- [확정 도면 v1](../Art/ProfessorHouse_ApprovedLayout_v1/README.md), 6쪽 PDF와 공간·문·서사 동선 데이터.

새 전체 연결 모형은 별도 다운로드 검토 작업으로 이어진다. 이번 기준점의 Unity 씬은 기존 v2 홀 구현이며, 확정 도면의 전체 1층·2층·다락 배치를 적용한 씬은 아니다.

## 이번 재검증

Unity 2022.3.62f3에서 현재 프로젝트를 직접 컴파일하고 검사했다. 과거 격리 프로젝트의 결과와 구분해 아래 기록을 보관한다.

| 항목 | 결과 | 근거 |
| --- | --- | --- |
| 구조·Chalk 계약 | 실패 0, 기존 스크립트 46개 GUID 보존 | [정적 검사](ProfessorHouseBaseline/StaticChecks.txt) |
| EditMode | 17개 통과, 실패·스킵 0 | [결과 XML](ProfessorHouseBaseline/EditMode.xml) |
| PlayMode | 6개 통과, 실패·스킵 0 | [결과 XML](ProfessorHouseBaseline/PlayMode.xml) |
| 홀 에셋·조명·프리팹·UV·NavMesh | 실패 0 | [씬 검사](ProfessorHouseBaseline/SceneValidation.txt) |
| 계단·문 접근 및 잠금 | 7개 경로 통과 | [이동 검사](ProfessorHouseBaseline/Navigation.txt) |
| 에셋 참조·형상 | 실제 씬 의존성과 환경 통계 기록 | [의존성](ProfessorHouseBaseline/SceneDependencies.txt) · [형상](ProfessorHouseBaseline/Geometry.txt) |
| 검사 대상 | 변경된 코드·에셋·생성 원본의 SHA-256 | [파일 스냅샷](ProfessorHouseBaseline/SourceSnapshot.json) |

PlayMode에는 실제 입력을 통한 계단 이동과 두 문 상호작용, 씬 재진입, Addressables 소유권 해제 검사가 포함된다. 이번에는 개발 플레이어 패키지를 다시 빌드하지 않았다. 기존 빌드·실행·화면 검증은 [2026-10-21 보고서](ProfessorHouseV2.md)의 당시 결과다.

확정 도면의 연결 검증과 후속 모형 작업은 기존 홀의 Unity 실행 검증과 별도로 관리한다. 검토 중인 새 모형은 다운로드 폴더에서 먼저 확인하고, 최종 결정한 자료를 프로젝트에 반영한다.
