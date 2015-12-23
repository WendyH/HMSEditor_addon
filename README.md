# HMSEditor Addon

HMSEditor Addon - это дополнение к в программе ["Домашний медиа-сервер"](https://www.homemediaserver.ru/), альтернативный редактор скриптов с IntelliSense и некоторыми другими возможностями.

Минимальная версия Home Media Server - 2.03, начиная с которой есть поддержка внешних дополнений.

Может быть полезен кодерам при написании скриптов.  

## Возможности

* Более богатая подсветка синтаксиса языков скрипта
* IntelliSense - подсказки ключевых слов, функций, классов, методов, переменных
* Подсказки для параметров функций
* Всплывающие подсказки по описанию функций, переменных и констант при наведении курсора мыши
* Полнее база по описанию встроенных функций, свойств и методов
* Подсветка одинаковых слов
* Замена по Ctrl-H
* Подсказка значений переменных и выделенной области при наведении курсора мыши во время отладки скрипта
* Использование шаблонов кода (возможность обновления их с GitHub)
* Автоматическая проверка скрипта на ошибки
* Поиск по справочнику описаний функций, классов, переменных и констант
* Форматирование кода
* Рефакторинг (переименование имён переменных и функций в видимых областях)
* Выбор цветовой темы оформления
* И некоторые другие....

## Инсталляция
* Скачиваете [последнюю версию дополнения](https://github.com/WendyH/HMSEditor_addon/releases/latest)
* В программе, в самом низу главной формы нажать на кнопку "Список дополнений", нажать кнопку "Добавить" и выбрать скачанный архив.

![](http://s9.postimg.org/3v3a9tp0f/hmseditor_addon_smple2.png)

## Горячие клавиши
* F2 - переименование переменной/функции (рефакторинг)
* F3 - найти следующее совпадение
* F5 - установка/снятие точки остановки (breakpoint)
* F7 - (только во время отладки) отображение окна "Вычислить выражение"
* F8 - пошаговая отладка
* F9 - запуск скрипта
* F11 - отображение/скрытие дополнительной панели инструментов
* F12 - Goto Definition - переход к определению переменной/функции
* Esc - скрытие всех подсказок
* Alt +1...9 - установка номерной закладки
* Ctrl+1...9 - переход к номерной закладке
* Ctrl+D - переключение на "родной" встроенный редактор (переключение назад - некоторое время ничего не делать)
* Alt+Влево - переход назад по истории переходов
* Alt+Вправо - переход вперёд по истории переходов
* Ctrl+Shift+C - закомментировать/раскомментировать выделенные строки
* Ctrl+F, Ctrl+H - показывает окно расширенного поиска и замены
* Ctrl+G - переход к к строке по номеру
* Ctrl+(C, V, X) - стандартные операции копировать/вставить/вырезать
* Ctrl+A - выбор всего текста
* Ctrl+Z, Alt+Backspace, Ctrl+R, Ctrl+Shift+Z - Отмена/Повтор действия
* Tab, Shift+Tab - увеличить/уменьшить отступ слева для выделенной области
* Ctrl+Home, Ctrl+End - переход в начало/конец текста
* Shift+Ctrl+Home, Shift+Ctrl+End - переход в начало/конец текста выделенной области
* Ctrl+Влево, Ctrl+Вправо - переход влево/вправо по словам
* Shift+Ctrl+Влево, Shift+Ctrl+Вправо - переход влево/вправо по словам с выделением области
* Ctrl+U, Shift+Ctrl+U - конвертирование выделенного текста в верхний/нижний регистр
* Ins - переключение режима вставки
* Ctrl+Backspace, Ctrl+Del - удаление слова слева/справа
* Alt+Mouse, Alt+Shift+(Ввехр, Вниз, Вправо, Влево) - включение режима выделения столбцов
* Alt+Вверх, Alt+Вниз - передвигание текущей строки целиком вверх/вниз
* Shift+Del - удаление текущей строки
* Ctrl+B, Ctrl+Shift-B, Ctrl+N, Ctrl+Shift+N - добавление, удаление и перемещение к закладкам
* Ctrl+Wheel - изменение масштаба
* Ctrl+M, Ctrl+E - включение/остановка записи макро (действий), запуск макро
* Alt+F [символ] - найти ближайший [символ]
* Ctrl+(Вверх, Вниз) - скроллирование вверх/вниз
* Ctrl+(NumpadPlus, NumpadMinus, 0) - увеличение масштаба, уменьшение масштаба, сброс масштаба в 100%

## Использование шаблонов
Нажав правой клавишей мышки на поле редактора, можно в появившемся меню выбрать пункт "Вставить шаблон". Где для конкретного языка скрипта будут варианты шаблонов с участками кода, которые можно выбрав, вставить в текст.

Наборы шаблонов выложены [тут](https://github.com/WendyH/HMSEditor-Templates), могут периодически обновляться и добавляться.  
Загрузить шаблоны с github для редактора можно в окне "О программе" (пункт в меню панели инструментов).

Можно создавать свои собственные шаблоны. Достаточно сохранить файл с кодом шаблона в папку %ProgramData%\HMSEditor\Templates в соответствующую подпапку языка кода.

Существующие шаблоны сделаны как пример или костяк, который необходимо редактировать под свои конкретные нужды и не является примером как именно нужно делать. Теоретически, могут содержать даже ошибки или неверный подход. И никак не могут являться решением на все случаи жизни.

Предложения по добавлению шаблонов в общую коллекцию приветствуются.

## Цветовые темы
Кроме встроенных цветовых тем, также можно добавлять свои файлы тем. Поддерживаются пока только темы совместимые с Sublime Text 2 / 3. Которые скачать, например, можно [отсюда](http://colorsublime.com/).

Чтобы добавить темы в редактор, нужно поместить файлы в директорию %ProgramData%\HMSEditor\Themes.

## Некоторые особенности
* Файлы настроек, загруженных шаблонов и тем хранятся в общей папке программ (в Windows 7 это "C:\ProgramData\HMSEditor\", в XP это "C:\Document and settings\All users\Application data\HMSEditor\").
* Проверка наличия обновлений шаблонов и программы делается только при открытии окна "О программе", запускается в фоне и занимает некоторое время.
* Все проверки делаются с использованием API GitHub.
* После загрузки файла обновления  редактора, проверяется наличие валидной цифровой подписи. Если не валидна - отказ от обновления.
* Т.к. GitHub работает только по протоколу https, то также проверяется наличие защищённого соединения (проверяется системой). Если сертификат использующийся для соединения не является доверенным - обновлений не будет. Идёт проверка на правильность соединения защищённого протокола. Это значит, если будет использоваться прокси с подставленным сертификатом или ваш провайдер тоже будет делать его подмену - соединения с GitHub не будет, а значит и обновлений. 
Из-за того, что настройки github.com безопасных протоколов соединения не поддерживают операционную систему Windowx XP - автоматически обновиться или скачать шаблоны под данной ОС не получиться. Только скачать вручную и обновить файл самостоятельно.
* "Умные" подсказки работают _только_ с языком PascalScript и C++Script. Ибо в JScript все переменные изначально с типом Variant и их объявление не обязательно.
* Анализ контекста не совершенен, делается на лету, через некоторое время после редактирования. Если синтаксис выбранного языка нарушен, то список видимых переменных и их типы для подсказок могут быть не адекватны.
* При нажатии клавиши Esc убираются все активные подсказки с экрана и не будут появляться до тех пор, пока небудет начат набор нового слова.

## Скриншоты  

#### Подсказки методов и свойств класса по типу переменной:  
![](http://s9.postimg.org/utxydhwpr/hmseditor_addon_smple1.png)

#### Подсказка по набранной части слова (тёмная тема):  
![](https://hms.lostcut.net/img/hmseditor/hmseditor_addon_ex1.png)

Пожелания, вопросы и комментарии можно оставить в ветке форума по этому адресу: https://hms.lostcut.net/viewtopic.php?id=131
