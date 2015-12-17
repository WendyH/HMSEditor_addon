using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace HmsAddons {
    public struct HmsAddonInfo {
        public Guid   ClassID;
        public Guid   InterfaceID;
        public string Title;
        public string Description;
        public string RequiredVersion;
        public string CheckedOnVersion;
    }

    public static class HRESULT {
        public const uint S_OK           = 0x00000000; // Operation successful
        public const uint E_ABORT        = 0x80004004; // Operation aborted
        public const uint E_ACCESSDENIED = 0x80070005; // General access denied error
        public const uint E_FAIL         = 0x80004005; // Unspecified failure
        public const uint E_HANDLE       = 0x80070006; // Handle that is not valid
        public const uint E_INVALIDARG   = 0x80070057; // One or more arguments are not valid
        public const uint E_NOINTERFACE  = 0x80004002; // No such interface supported
        public const uint E_NOTIMPL      = 0x80004001; // Not implemented
        public const uint E_OUTOFMEMORY  = 0x8007000E; // Failed to allocate necessary memory
        public const uint E_POINTER      = 0x80004003; // Pointer that is not valid
        public const uint E_UNEXPECTED   = 0x8000FFFF; // Unexpected failure
    }

    public static class Constatns {
        // IHmsScriptEditor.GetCapabilities
        public const int ecEditor    = 1;
        public const int ecStatusBar = 2;
        public const int ecMessages  = 4;

        // IHmsScriptFrame.ProcessCommand
        public const int ecUserFirst        = 1001;
        public const int ecCompileScript    = ecUserFirst + 103;
        public const int ecRunLine          = ecUserFirst + 100;
        public const int ecRunScript        = ecUserFirst + 102;
        public const int ecToggleBreakpoint = ecUserFirst + 101;
        public const int ecEvaluate         = ecUserFirst + 104;
        public const int ecWatches          = ecUserFirst + 105;

        public const int ufCheckExistsOnly  = 0x01; // проверить существование обновления (in)
        public const int ufIsHttpLink       = 0x02; // результат - http-ссылка на zip-файл дополнения (out)
        public const int ufIsLocalFile      = 0x04; // результат - скаченный zip-файл дополнения (out)
        public const int ufIsInfoMessage    = 0x08; // результат - информационное сообщение (например, что обновление прошло успешно) (out)
        public const int ufIsErrorMessage   = 0x10; // результат - сообщение об ошибке (out)
        public const int ufIsWarningMessage = 0x20; // результат - сообщение, требующее внимания (например, что требуется перезагрузка программы) (out)

        public const string CLASS_HmsAddonList = "1C6BC2D4-5AF8-4203-98D3-5D4CA48E6C6F";

        public static Guid Guid(Type t) {
            Attribute guidAttribute = Attribute.GetCustomAttribute(t, typeof(GuidAttribute));
            return new Guid(((GuidAttribute)guidAttribute).Value);
        }

        public static bool IsEqualGUID(Guid guid1, Type t) {
            return (guid1.CompareTo(Guid(t)) == 0);
        }
    }

    public enum HmsScriptMode {
        smUnknown = 0,             // Просто скрипт
        smTranscoding,             // Профиль транскодирования
        smWatchFoldersGroupName,   //
        smMediaInfo,
        smProcessMedia,            // Скрипт обработки медиа-ресурсов
        smCreateFolderItems,       // Скрипт создания подкаст-лент
        smPodcastItemProperties,   // Скрипт чтения доролнительных свойств RSS
        smMediaResourceLink,       // Скрипт получения ссылки на ресурс
        smWebMediaItems,
        smMimeType,                // Скрипт определения MIME-типа
        smProcessMediaEvent,
        smCreatePodcastFeeds,      // Скрипт чтения списка ресурсов
        smWebNavigation,           // Создание страниц Web-навигации
        smParsePlaylistFile,
        smAutoDetectDeviceType,    // Автоопределение типа устройства
        smProcessMetadata,         // Скрипт загрузки файла метаданных
        smDIDLLiteDescription,     // Формирование DIDL-Lite описания медиа-ресурсов
        smHandleHTTPRequest        // Обработка HTTP-запросов
    }

    // Для вызова из списка дополнений
    [ComVisible(true), Guid("C5B24BFB-1F30-4F8A-91AD-943B82D8A067")]
    public interface IHmsAddonTools {
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        uint Setup(IntPtr aParent, ref int aReload);

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        uint Update(ref int aFlags, ref object aResult);
        /* Результатом (aResult) может быть ссылка на zip-файл обновления, загруженный zip-файл обновления, результат обновления.
           Флаги (aFlags) будут определять тип результата (Out) или тип вызова (In) Update (например, проверить наличие новой версии без загрузки обновления).
        */
    }

    // Для получения списка классов и интерфейсов
    [ComVisible(true), Guid("A8F688A7-441E-4701-9EA0-9C591D0B997A")]
    public interface IHmsAddonList {
        uint GetCount(ref int aCount);
        uint GetAddonInfo(int aIndex, ref Guid aClassID, ref Guid aInterfaceID, ref object aTitle, ref object aDescription, ref object aRequiredVersion, ref object aCheckedOnVersion);
        uint GetClassObject(ref Guid clsid, ref Guid iid, out object instance);
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        uint CanUnloadNow ();
    }

    // Интерфейс редактора
    [ComVisible(true), Guid("B43BB779-379D-4244-A53D-0AAC3863A0FB")]
    public interface IHmsScriptEditor {
        uint AddMessage(ref object aMessage);

        uint CreateEditor(IntPtr THandle, IntPtr aScriptFrame, int aScriptMode, ref IntPtr aEditor);
        uint DestroyEditor(IntPtr aEditor);

        uint GetCapabilities(ref int aCapabilities);

        uint GetCurrentLine(ref int aLine);

        uint GetScriptName(ref object aScriptName);
        uint GetScriptText(ref object aText);
        uint SetScriptName(ref object aScriptName);
        uint SetScriptText(ref object aText);

        uint GetModified(ref int aModified);

        uint InvalidateLine(int aLine);

        uint Repaint();

        uint GetCaretPos(ref int aLine, ref int aChar);
        uint SetCaretPos(int aLine, int aChar);

        uint SetFocus();

        uint SetRunning(int aValue); 

        uint SetSelText(ref object aText);

        uint Setup();
    }

    // Интерфейс программы, который предоставляется редактору
    [ComVisible(true), Guid("D31B4638-9764-4A9A-9F5A-B4D0B519F402")]
    public interface IHmsScriptFrame {
        uint AddWatch(ref object aExpression);

        uint ChangeScriptName(ref object aScriptName);

        [MTAThread]
        uint CompileScript(ref object aScriptName, ref object aScriptText, ref object aErrorMessage, ref int aErrorLine, ref int aErrorChar, ref int aResult);

        uint GenerateScriptDescriptions(ref object aXMLDescriptions);

        uint GetCurrentState(ref int aRunning, ref int aCurrentSourceLine, ref int aCurrentSourceChar);

        uint IsBreakpointLine(int aLine, ref int aResult);
        uint IsExecutableLine(int aLine, ref int aResult);

        uint ProcessCommand(int aCommand);
        uint SolveExpression(ref object aExpression, ref object aResult);

        uint ToggleBreakpoint(int aLine);
    }

    // Интерфейс дополнений, расширяющих список функций встроенных скриптов
    [ComVisible(true), Guid("99546421-6D4B-4BEB-B778-285591F25D79")]
    public interface IHmsScriptFunctions {
        uint ExecuteFunction (ref object aFunctionName, ref object aFunctionParameters, ref object aFunctionResult);
        uint GetFunctionCount(ref int aCount);
        uint GetFunctionInfo (int aIndex, ref object aFunctionName, ref object objectaFunctionDeclaration, ref object objectaFunctionDescription);
    }

    // Интерфейс дополнений, расширяющих список функций встроенных скриптов
    [ComVisible(true), Guid("6E86502E-15BC-4358-A4A1-29D97CDEF073")]
    public interface IHmsMainFormControl {
        uint CreateControl (IntPtr aParent, ref object aReserved, ref IntPtr aControl);
        uint DestroyControl(IntPtr aControl);

        uint GetCaption(ref object aCaption);
        uint SetFocus();

        uint ExecuteAction (ref object aActionName, ref object aActionParameters);
        uint GetActionCount(ref int aCount);
        uint GetActionInfo (int aIndex, ref object aActionName, ref object aActionTitle, ref object aActionDescription);
    }


}
