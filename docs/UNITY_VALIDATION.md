# EV-CP1 — Unity Integration & Validation

**Статус: файлы интегрированы; настоящая компиляция/проверка Unity НЕ выполнена.**
Целевая версия: **6000.6.10f1**, без обновления/понижения. Следующая игровая система
не начинается до пользовательской проверки этого этапа.

## Что интегрировано и почему так

Runtime-копия находится в `Assets/Evolution/EVCP1/`:

- `Code/Core/`: Morphology.cs, RadialGeometry.cs, ContourValidator.cs, PlanarMotion.cs,
  Evolution.Source.Core.asmdef (`noEngineReferences: true`).
- `Code/Unity/`: CreatureBody2D.cs, CreatureMotor2D.cs, CreatureKeyboardInput2D.cs,
  MorphologyDemo2D.cs, VectorObstacle2D.cs, EvolutionPrototype2D.cs,
  Evolution.Source.Unity.asmdef (ссылка на Core по имени, без выдуманного GUID).
- `Art/Resources/EvolutionVector2D.shader`: единственный ресурс с ключом
  `EvolutionVector2D`, который загружают оба новых компонента.

13 файлов скопированы без изменений содержимого. Канонические Code/Art остаются в
Evolution_Source для standalone-проверок; Unity не импортирует эту папку вне Assets.
CI проверяет побайтовое равенство зеркала. Не редактировать две копии независимо.
Standalone/PlayMode-тесты, Python, SDK/Roslyn и CI-инструменты в Assets НЕ переносились.

Старые `Assets/Evolution/Scripts/Creature/ProceduralCreature2D.cs` и
`Assets/Editor/EvolutionDemoSceneBuilder.cs` сохранены. В первом исправлено только
некорректное объявление RequireComponent с четырьмя аргументами: оно разделено на
два допустимых атрибута. Остальное поведение старого компонента не переписано.
Старое меню **Tools → Evolution → Create Morphology Demo** создаёт СТАРУЮ демонстрацию,
а не EV-CP1. Не используйте его для приёмки новой реализации.

Конфликтов новых имён/пространств имён/asmdef при статическом аудите не найдено:
legacy = Evolution.Creatures / Evolution.EditorTools; новая реализация =
Evolution.Source.Core / Evolution.Source.Unity. Не добавляйте старый и новый
компоненты на один GameObject: они управляют одними MeshFilter/Collider слотами.

В исходном Packages/manifest.json зависимости были пусты. Добавлен только штатный
модуль Editor `com.unity.modules.physics2d: 1.0.0`, необходимый для Rigidbody2D /
PolygonCollider2D, а не сторонний пакет или новая физическая система. URP, Input
System и Test Framework не устанавливались. ProjectVersion.txt не изменён.
Других ProjectSettings-файлов в исходном репозитории не было; первые настройки
Editor создаст локально. Их не генерировали и не подменяли вручную.

## 1. Открытие и импорт — выполнить локально

1. Получите актуальный main. В Unity Hub добавьте СУЩЕСТВУЮЩУЮ корневую папку
   репозитория с Assets, Packages, ProjectSettings (не Evolution_Source).
2. Откройте строго Unity 6000.6.10f1. Не соглашайтесь на миграцию версии.
3. Дождитесь Package Manager/import/компиляции. Убедитесь, что штатный Physics 2D
   разрешился; при ошибке сохраните полный текст, не обновляйте пакеты наугад.
4. Очистите Console и проверьте отсутствие C#-ошибок, shader errors, missing scripts
   и исключений импорта. Проверьте две сборки Evolution.Source.Core и
   Evolution.Source.Unity. CI НЕ проверяет связывание с UnityEngine API.
5. Дайте Unity сгенерировать .meta для новых файлов и папок; сохраните их. Существующие
   локальные .meta не удалять/не заменять. После первого импорта закоммитьте новые
   .meta вместе с созданной ниже сценой и её .meta. Ни одного GUID здесь не выдумано.
6. Проверьте Built-in Render Pipeline: никакой URP/HDRP-пакет не установлен, а графические
   настройки ранее отсутствовали. Не меняйте pipeline ради этой проверки. Если ваша
   локальная копия уже настроена иначе, сообщите об этом — совместимость не заявлена.

## 2. Минимальная сцена — автоматическое движение и контакт

1. Создайте **новую пустую сцену** через File → New Scene → Empty. Не заменяйте и не
   сохраняйте поверх чужой сцены. Удалите только автоматически добавленные камерy/свет
   из этой новой тестовой сцены, если выбранный шаблон их создал.
2. Создайте пустой GameObject `EVCP1 Bootstrap`; Transform: position (0,0,0), rotation
   (0,0,0), scale (1,1,1).
3. Добавьте компонент **EvolutionPrototype2D** из Assets/Evolution/EVCP1/Code/Unity.
   Оставьте **Use Keyboard Controls = false**. Не добавляйте другие компоненты существа
   на bootstrap: фикстура создаст их в Start во время Play.
4. Сохраните новую сцену как `Assets/Evolution/EVCP1/Scenes/EVCP1_Validation.unity`
   (папку Scenes создайте в Editor). Она получит настоящие GUID от Unity.
5. Перед Play проверьте Time Scale = 1, Fixed Timestep = 0.02; Physics 2D Simulation Mode
   = Fixed Update и разрешённую коллизию Default ↔ Default. Это ручная проверка;
   настройки не перезаписаны агентом.
6. Нажмите Play. Ожидаемая иерархия: bootstrap плюс runtime-root
   `EV-CP1 Source Prototype` с одной ортографической камерой, `Source Creature`
   и `Single Vector Obstacle`. Root создан в runtime и не должен сохраняться в сцену.
7. Существо начинает в (-3,0), препятствие в (0.8,0), камера в (0,0,-10), size 4.5.
   Default intent направлен вправо. Должно быть видно постепенное приближение к стенке,
   остановку/контакт, а не проход через неё. Подождите 30–60 секунд для морфинга.
8. В режиме Scene включите Gizmos/контур PolygonCollider2D. На существе ожидаются
   MeshFilter, MeshRenderer, PolygonCollider2D, Rigidbody2D, CreatureBody2D,
   CreatureMotor2D и MorphologyDemo2D; на препятствии — mesh + polygon без Rigidbody2D.
9. Rigidbody2D: Dynamic, Simulated, Gravity Scale 0, Freeze Rotation, Continuous,
   Interpolate. Движение идёт через linearVelocity в FixedUpdate. Не двигайте Transform
   динамического тела вручную во время проверки контакта.
10. Остановите Play. Runtime-root должен исчезнуть; повторный Play должен создать только
    один root/одно существо/одно препятствие, без накопления объектов и ошибок.

## 3. Отдельная проверка клавиатуры

1. Выйдите из Play. На bootstrap включите **Use Keyboard Controls** до следующего Play
   (переключение этого поля после Start не перестраивает ввод).
2. Проверьте Player → Active Input Handling: требуется legacy Input Manager или Both.
   Input System пакет для этой проверки не нужен и агентом не добавлен. Если legacy
   недоступен, клавиатурный компонент должен выдать предупреждение и отключиться;
   автоматическое движение при этом не подменяет управление.
3. Нажмите Play и сфокусируйте Game view. Проверьте WASD и стрелки, движение по XY,
   диагональ без увеличения скорости, торможение после отпускания, упор в препятствие
   с обеих сторон. Автоматического right-intent в этом режиме быть не должно.
4. Оставьте масштаб тела единичным, Z=0; не оценивайте клавиатуру при Time Scale 0.

## 4. Чек-лист приёмки и регрессий

- [ ] Console без ошибок C#/shader/runtime; первый импорт завершён на 6000.6.10f1.
- [ ] Resources.Load<Shader>("EvolutionVector2D") возвращает shader, Shader.isSupported
      true; силуэты не розовые/не пропадают. Проверить для тела И препятствия.
- [ ] Один замкнутый силуэт; нет мерцания, скачков, разрывов и самопересечений при морфинге.
- [ ] Mesh и PolygonCollider2D совпадают во время изменения формы и у препятствия.
- [ ] Контакт не даёт заметного проникновения/туннелирования, выброса скорости и дрожания
      при росте/сжатии формы. Проверить несколько подходов и длительный упор.
- [ ] Торможение и клавиатура работают; 2D-коллизия включена и не является Trigger.
- [ ] Отключение/включение bootstrap при Play приостанавливает/возобновляет его root,
      не создаёт дубль; отключение/включение CreatureBody2D не сбрасывает форму.
- [ ] Удаление body/obstacle-компонента отключает его collider и освобождает собственные
      mesh/material; внешние материалы не уничтожаются. Не судить по GC alone о native.
- [ ] Rigidbody2D, добавленный после Start тела, просыпается при следующей смене геометрии.
- [ ] Неизменная цель: отключить MorphologyDemo2D и задать текущую форму как target;
      после стабилизации GeometryRevision не растёт. Проверить в debugger; runtime HUD
      и отдельная игровая система ради диагностики не добавлялись.
- [ ] Profiler: проверить native Mesh/Collider publication и managed allocations после
      прогрева. Нулевые аллокации standalone-ядра НЕ доказывают ноль в Unity API.
- [ ] Некорректный материал без _Color / отсутствующий shader: на отдельном неактивном
      тестовом объекте настроить ошибку, затем активировать. Инициализация должна завершиться
      ошибкой с IsInitialized=false, отключённым render/collision, без утечки частичных
      ресурсов и повторяющихся исключений каждый FixedUpdate. Исправить конфигурацию
      и пересоздать компонент; enabled toggle не предназначен для retry после failed Awake.

Пять подготовленных PlayMode-тестов остаются в Evolution_Source/Tests/Unity/PlayMode
и **не импортированы/не выполнены**. Их запуск — отдельная локальная операция после
обеспечения Unity Test Framework; пакет без необходимости не устанавливался.
Ручная проверка выше не зависит от него.

## Известные ограничения

- Агент НЕ запускал Unity Editor, Play Mode, shader compiler или Unity Build. Успех CI
  означает standalone C#/Python/синтаксис, а не компиляцию проекта в Unity.
- Shader рассчитан на текущую Built-in baseline; SRP/URP/HDRP не валидированы.
- Подготовлена инструкция, а не вручную сериализованный .unity/.prefab с выдуманными GUID.
- Первый импорт создаст недостающие .meta и настройки. Проверка отсутствующих ссылок
  до реальной генерации GUID ограничена; существующих .meta в исходном Assets не было.
- Старый ExecuteAlways-компонент и его меню сохранены, но не являются новым путём приёмки
  и не подвергались повторной Unity-валидации. Совместное размещение на одном объекте
  с новой реализацией не поддерживается.
- Одно существо/одна статическая стенка; морфинг — демонстрация, не обучение/адаптация.
- Физика при изменении геометрии, depenetration/CCD и визуальная плавность ещё не приняты.

## Что прислать после проверки

Commit SHA, точную версию Editor, результат каждого пункта, полный первый Console error
со stack trace (если есть), pipeline/Input Handling, короткое видео морфинга/контакта и
новые Unity-generated .meta/сцену отдельным коммитом. До этого следующая игровая система
не начинается и EV-CP1 не объявляется полностью завершённым.


## User-confirmed shader correction — 2026-11-24

The first local Play Mode inspection exposed a Built-in pipeline shader-pass mismatch:
`EvolutionVector2D.shader` used `LightMode=SRPDefaultUnlit`. The user changed that tag
to `Always` in the integrated Assets copy and confirmed that the filled green creature
and gray obstacle then rendered. The canonical source and runtime mirror must stay
byte-identical; the repository synchronization commit records the same correction in
both locations. This is a rendering fix only, not a biological simulation milestone.
