using System.Collections.Generic;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
	/// <summary>
	/// Класс для хранения элемента загруженного шаблона (файла или секции из INI) (язык, имя, текст или это субменю/подкатолог)
	/// </summary>
	public class TemplateItem {
		public Language  Language   = Language.BasicScript;
		public string    Name       = "";
		public string    Text       = "";
		public Templates ChildItems = new Templates();
		public bool      Submenu { get { return ChildItems.Count > 0; } }

		// constructor
		public TemplateItem() {
		}
		// constructor
		public TemplateItem(Language lang) {
			Language = lang;
		}
	}

	/// <summary>
	/// Класс для хранения набора загруженных шаблонов кода для редактора
	/// </summary>
	public class Templates: List<TemplateItem> {

		/// <summary>
		/// Можно получить выборку из набора по указанному языку
		/// </summary>
		/// <param name="lang">Язык скрипта</param>
		/// <returns>Возвращает набор шаблонов для выбранного языка скрипта</returns>
		public Templates this[Language lang] {
			get {
				Templates items = new Templates();
				foreach (var o in this) if (o.Language == lang) items.Add(o);
				return items;
			}
		}

		/// <summary>
		/// Получение элемента шаблона из набора по его имени
		/// </summary>
		/// <param name="name">Имя шаблона</param>
		/// <returns>Возвращает элемент шаблона</returns>
		public TemplateItem this[string name] {
			get {
				foreach (var o in this) if (o.Name == name) return o;
				TemplateItem item = new TemplateItem();
				item.Name = name;
				Add(item);
				return item;
			}
		}

		/// <summary>
		/// Установка нового (или существуюющего элемента с таким именем) шаблона с указанным именем и содержимым
		/// </summary>
		/// <param name="lang">Язык скрипта</param>
		/// <param name="name">Имя шаблона</param>
		/// <param name="text">Содержание шаблона</param>
		/// <returns></returns>
		public TemplateItem Set(Language lang, string name, string text = "") {
			string namel = name.ToLower();
			foreach (var o in this) {
				if (o.Language == lang && o.Name.ToLower() == namel) {
					o.Name = name;
					o.Text = text;
					return o;
				}
			}
			TemplateItem item = new TemplateItem(lang);
			item.Name = name;
			item.Text = text;
			Add(item);
			return item;
		}
	}
}
