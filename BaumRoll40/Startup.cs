using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

//[assembly: OwinStartup(typeof(BaumRoll40.Startup))]

namespace BaumRoll40
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // アプリケーションの構成方法の詳細については、https://go.microsoft.com/fwlink/?LinkID=316888 を参照してください
            //ConfigureAuth(app);
            app.MapSignalR(); // ← この行を追加
        }
    }
}
