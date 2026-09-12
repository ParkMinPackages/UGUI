## 의존 패키지

#본 패키지는 다음의 필수 외부 의존성이 있습니다.
1. com.parkminpackages.foundation
2.com.cysharp.unitask

#본 패키지는 DOTween용 UIAnimation을 활성화 할 수 있습니다.
1.Dotween 설치
2.Scripting Symbol 추가 : `DOTWEEN`, `UNITASK_DOTWEEN_SUPPORT`
3.Tools > Demigiant > DOTween Utility Panel 에서 AssemblyDefinition 추가

#본 패키지는 Litmotion용 UIAnimation을 활성화 할 수 있습니다.
1.그냥 Litmotion이 패키지매니저에 설치되어 있기만 하면 됩니다.

## PSD Converter

상단 메뉴 **ParkMinPackages > PSD Converter**에서 PSD 레이어를 Canvas로 변환할 수 있습니다.

### 사용 순서

1. **PSD 에셋**을 눌러 Unity Search에서 PSD를 선택하거나 프로젝트의 PSD를 드래그한 뒤 **분석**을 누릅니다. 분석 성공 후 설정 영역이 표시됩니다.
2. Canvas 기준 해상도를 정합니다. 기본값은 PSD 문서 크기입니다.
3. **PSD Text -> Unity Text 설정**에서 텍스트 방식(레거시/TMP), 미연결 폰트 처리, 기본대체폰트 및 폰트 저장 경로를 지정합니다.
4. PSD에 기록된 PostScript 폰트 이름별로 Unity Font / TMP Font Asset을 연결합니다.
5. 레이어 목록을 펼쳐 이미지로 유지할 레이어를 선택한 뒤 **변환하고 현재 씬에 배치**를 누릅니다.

현재 씬 루트에 문서명_Legacy 또는 문서명_TMP Canvas가 새로 생성됩니다.
원본 PSD와 기존 씬 오브젝트는 변경하지 않으며, 생성 작업은 Undo로 취소할 수 있습니다.
씬 저장은 사용자가 직접 수행합니다.

선택창은 .psd 파일만 표시하며 이름과 경로로 검색할 수 있습니다. 비동기 검색과 목록 표시를 사용하고 Inspector 미리보기는 비활성화합니다. 선택 취소 시 기존 값은 유지되고, PSD 분석은 **분석** 버튼으로 따로 실행합니다. 검색어에서 확장자 조건을 지워도 결과는 PSD로 제한됩니다.

### 폰트 처리

- 자동 연결은 하지 않습니다. **기존 폰트 연결** 또는 후보가 여러 개인 경우 **기존 폰트 선택**으로 직접 연결합니다.
- **폰트 목록 새로고침**은 검색 목록만 갱신하며 기존 연결값을 변경하지 않습니다.
- **시스템 폰트 가져오기**는 사용자가 라이선스 안내를 확인한 뒤 접근 가능한 TTF/OTF 원본을 Assets 아래 저장 폴더로 가져옵니다. 시스템 경로는 이 버튼 상태에서만 표시됩니다.
- Adobe Fonts 활성화 캐시 및 TTC/OTC 컬렉션에서 개별 폰트를 추출하는 기능은 제공하지 않습니다.
- TMP 방식에서는 **TMP 폰트 준비 및 연결**로 원본 Font에 대응하는 에셋을 재사용하거나 생성합니다. 시스템 가져오기 시에는 원본을 가져온 뒤 TMP를 준비합니다. 생성에는 TMP Essential Resources가 필요합니다.
- 동일 파일은 재사용하며, 새 파일 생성 시 기존 파일을 덮어쓰지 않습니다.
- 미연결 폰트는 기본 대체 폰트 사용 또는 이미지 유지 중 선택합니다. 대체 폰트도 없으면 이미지를 유지합니다.
- 레이어별 이미지 유지 토글은 폰트 정책과 독립적으로 항상 표시되며 PSD 식별자와 레이어 ID별로 저장됩니다. 변환 미지원 레이어는 체크된 비활성 토글과 노란 안내로 표시됩니다.
- 설정과 폰트 연결은 프로젝트별로 저장됩니다. 공통 설정은 **Project Settings > ParkMinPackages > PSD Converter**에서도 수정할 수 있습니다.

### 지원 범위와 제한

- 현재 Unity PSD Importer 14.x의 레이어 정보와 RGB / 8 bits PSD 메타데이터를 대상으로 구현했습니다. 선택창은 .psd만 허용하며 레이어별 Sprite 임포트가 필요합니다.
- 이미지 레이어는 임포트된 Sprite를 재사용하고, 레이어 순서·그룹·표시 상태를 반영합니다.
- 단일 스타일의 가로 한 줄 텍스트는 편집 가능한 Text 또는 TextMeshProUGUI로 변환합니다.
- 혼합 스타일, 여러 줄/문단, 워프, 레이어 효과·마스크 등 직접 변환하지 않는 텍스트는 이미지로 유지하고 경고를 표시합니다.
- Photoshop과 Unity의 글자 치수·기준선 계산 차이로 텍스트 위치와 크기를 미세 조정해야 할 수 있습니다.
- 그룹 효과, 클리핑 및 Photoshop 전용 블렌딩의 완전한 재현은 보장하지 않습니다. 이런 레이어는 PSD Importer에서 이미지로 병합해 사용하는 것이 적합합니다.
- 외부 Python 실행 환경, Photoshop 설치 또는 Adobe 로그인은 필요하지 않습니다.

메타데이터 형식 참고: [Adobe Photoshop File Formats Specification](https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/), [OpenType Naming Table](https://learn.microsoft.com/en-us/typography/opentype/spec/name).
