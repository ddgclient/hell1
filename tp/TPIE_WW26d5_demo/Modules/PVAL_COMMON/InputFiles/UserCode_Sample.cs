namespace PVAL_COMMON
{
	using System;
	using Prime.ConsoleService;
	using Prime.DatalogService;
    using Prime.DatalogService.DatalogSpec;
	public class DummySample
	{
		public string LogTest()
		{
			var ituffWriter = Prime.Services.DatalogService.GetItuffStrgvalWriter();
			ituffWriter.SetTnamePostfix("_UserCodeTC");
			ituffWriter.SetData("DummyValue");
			Prime.Services.DatalogService.WriteToItuff(ituffWriter);

			return "1";
		}
	}
}
