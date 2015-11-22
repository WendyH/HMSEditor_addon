using System;
using System.Runtime.InteropServices;

namespace HMS.Addons {
	struct HmsAddonInfo {
		public Guid   ClassID;
		public Guid   InterfaceID;
		public string Title;
		public string Description;
		public string RequiredVersion;
		public string CheckedOnVersion;
	}

	static class HRESULT {
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

	static class Constatns {
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

		public const string CLASS_HmsAddonList = "1C6BC2D4-5AF8-4203-98D3-5D4CA48E6C6F";

		public static Guid Guid(Type t) {
			Attribute guidAttribute = Attribute.GetCustomAttribute(t, typeof(GuidAttribute));
			return new Guid(((GuidAttribute)guidAttribute).Value);
		}

		public static bool IsEqualGUID(Guid guid1, Type t) {
			return (guid1.CompareTo(Guid(t)) == 0);
		}
	}

	enum HmsScriptMode {
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

	// Для получения списка классов и интерфейсов
	[Guid("A8F688A7-441E-4701-9EA0-9C591D0B997A"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IHmsAddonList {
		uint GetCount(ref int aCount);
		uint GetAddonInfo(int aIndex, ref Guid aClassID, ref Guid aInterfaceID, ref object aTitle, ref object aDescription, ref object aRequiredVersion, ref object aCheckedOnVersion);
	}

	// Основной интерфейс всех дополнений
	[Guid("5C75011A-1A44-47DD-A382-5D829DC24F28"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IHmsCustomAddon {
		uint GetDescription(ref object aDescription);
		uint GetTitle(ref object aTitle);
		uint GetType(ref Int64 aType);
	}

	// Интерфейс редактора
	[Guid("B43BB779-379D-4244-A53D-0AAC3863A0FB"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IHmsScriptEditor: IHmsCustomAddon {
		uint AddMessage(object aMessage);

		uint CreateEditor(IntPtr THandle, IHmsScriptFrame aHmsScripter, int aScriptMode, ref IntPtr aEditor);
		uint DestroyEditor(IntPtr aEditor);

		uint GetCapabilities(ref int aCapabilities);

		uint GetCurrentLine(ref int aLine);

		uint GetScriptName(ref object aScriptName);
		uint GetScriptText(ref object aText);
		uint SetScriptName(object aScriptName);
		uint SetScriptText(object aText);

		uint GetModified(ref bool aModified);

		uint InvalidateLine(int aLine);

		uint Repaint();

		uint GetCaretPos(ref int aLine, ref int aChar);
		uint SetCaretPos(int aLine, int aChar);

		uint SetFocus();
		uint SetSelText(object aText);

		uint Setup();
	}

	// Интерфейс программы, который предоставляется редактору
	[Guid("D31B4638-9764-4A9A-9F5A-B4D0B519F402"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IHmsScriptFrame {
		uint AddWatch(object aExpression);

		uint ChangeScriptName(object aScriptName);

		bool CompileScript(object aScriptName, object aScriptText, ref object aErrorMessage, ref int aErrorLine, ref int aErrorChar);

		long GenerateScriptDescriptions(ref object aXMLDescriptions);

		uint GetCurrentState(ref bool aRunning, ref bool aStepByStep, ref bool aStopped, ref int aCurrentSourceLine, ref int aCurrentSourceChar);

		uint IsBreakpointLine(int aLine, ref bool aResult);
		uint IsExecutableLine(int aLine, ref bool aResult);

		uint ProcessCommand(int aCommand);
		uint SolveExpression(object aExpression, ref object aResult);

		uint ToggleBreakpoint(int aLine);
	}

}
