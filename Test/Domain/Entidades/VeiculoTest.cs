using MinimalApi.Dominio.Entidades;

namespace Test;

[TestClass]
public class VeiculoTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //Arrange
        var veiculo = new Veiculo();

        //Act
        veiculo.Id = 1;
        veiculo.Ano = 2013;
        veiculo.Marca = "teste";
        veiculo.Nome = "teste";


        //Assert
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual(2013, veiculo.Ano);
        Assert.AreEqual("teste", veiculo.Marca);
        Assert.AreEqual("teste", veiculo.Nome);

    }
}