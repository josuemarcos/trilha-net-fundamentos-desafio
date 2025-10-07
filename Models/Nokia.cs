namespace DesafioPOO.Models
{
    public class Nokia : Smartphone
    {
        public Nokia(string numero, string modeloFornecido, string imeiFornecido, int memoriaInformada) : base(numero, modeloFornecido, imeiFornecido, memoriaInformada)
        {

        }
        public override void InstalarAplicativo(string nomeApp)
        {
            System.Console.WriteLine($"Instalando {nomeApp} no celular Nokia");
        }
    }
}
