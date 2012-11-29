﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Common.Logging;
using CtcApi.Extensions;
using CtcApi.Web.Mvc;

namespace DataExport.WS.Controllers
{
	/// <summary>
	/// 
	/// </summary>
  public class ApiController : BaseController
	{
		private ILog _log = LogManager.GetCurrentClassLogger();
		// (ExporterConfig)ConfigurationManager.GetSection(ExporterConfig.GetSectionName())
		// TODO: replace the following initializations with loading from config settings (above)
		private IList<IExporter> _exporters = new List<IExporter>
		                                      	{
		                                      			new Exporter
		                                      				{
		                                      						Name = "maxient1",
		                                      						Data = new SqlDataInput {CmdText = "SELECT * FROM vw_Maxient_Feed1"},
																											Format = new CsvFormat {Separator = "|"}
		                                      				},
																								new Exporter
																									{
																											Name = "maxient2",
																											Data = new SqlDataInput {CmdText = "SELECT * FROM vw_Maxient_Feed2 ORDER BY [SID]"},
																											Format = new XslFormat
																											         	{
																											         			TemplateFile = "Maxient2.StudentSchedule.xslt"
																											         	}
																									}
  	                                      	};

		#region Initialization
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// This constructor initializes the <see cref="BaseController"/>, which provides common
		/// functionality like populating <i>ViewBag.Version</i> with the <see cref="Version"/>
		/// of the current MVC application.
		/// </remarks>
		public ApiController() : base(Assembly.GetExecutingAssembly())
		{
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
    public ActionResult Index()
    {
			_log.Trace(m => m("=> ApiController::Index()"));

      return View();
    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ActionResult Export(string id)
		{
			IEnumerable<IExporter> exporters =  _exporters.Where(x => x.Name.ToUpper() == id.ToUpper());
			int exporterCount = exporters.Count();
			
			if (exporterCount > 0)
			{
				if (_log.IsWarnEnabled && exporterCount > 1) {
					_log.Warn(m => m("Multiple exporters found with the name '{0}' - only one will be used.", id));
				}

				IExporter exporter = exporters.Take(1).Single();

				string csv = exporter.Export();
				_log.Trace(m => m("Export result:\n{0}", csv));

				if (string.IsNullOrWhiteSpace(csv))
				{
					_log.Warn(m => m("Exporter '{0}' returned an empty string.", id));
				}
				else
				{
					// TODO: implement uploading via SSH

			
					// TODO: add config settings for saving file
					bool saved = csv.ToFile("output.txt");

					// TODO: provide option to return output filestream
					return View("ExportResult", saved);
				}
			}
			else
			{
				_log.Warn(m => m("No exporter was found for '{0}'", id));
			}
			return View("ExportResult", false);
		}
  }
}
