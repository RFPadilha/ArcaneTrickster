# Arcane Trickster

Projeto criado para avançar o campo científico de geração procedural. Gera os mapas de forma procedural de acordo com diversos parâmetros, incluindo itens, inimigos, salas e chefes. Além disso o projeto tem a finalidade de coletar dados sobre como o jogador escolheu progredir através do jogo, como por exemplo: precisão de ataques, tempo decorrido, inimigos derrotados, itens coletados, se encontrou o item escondido, missões cumpridas, etc. Esses dados são utilizados para perfilar o jogador em busca de personalizar a geração do mapa com a finalidade de melhorar o engajamento do usuário.

## Unity 2021.3.11f1
Para importar o projeto no seu editor local, crie um novo projeto no Unity 2021.3.11f1, clone o repositório em sua máquina local e importe os arquivos no projeto. Se decidir usar o GitHub Desktop, é recomendável deixar separado a pasta usada pelo Git Desktop e a pasta do projeto do unity, assim qualquer alteração feita no repositório não interfere com o projeto local.

## Hierarquia de cenas:
Para rodar o projeto corretamente, as seguintes cenas devem ser carregadas na Hierarquia, em ordem:
### 0-Preload
- contém somente o objeto GameManager, que é o único objeto de jogo que deve ficar ativo através de todas as cenas, por isso é necessário que seja carregado primeir
- controla todos os sons, acompanha missões, salva dados da geração e é o que repassa os dados para o questionário através do script "DataGenerator".

### 1-Game
- Cidade inicial, onde se encontram os NPCs que atribuem missões. Existe um objeto escondido ao norte (eixo Y positivo) que é uma melhoria de HP, buscando entender o quanto o jogador explora os cenários.
- A cena conta com um portal roxo, que, ao entrar em contato com o personagem do jogador, abre o menu de geração do mapa, podendo gerar um mapa com as configurações padrões, ou permitindo a edição dos parâmetros.

### 2-MapGeneration
- Cena do mapa gerado proceduralmente, é onde o jogador cumpre as missões que aceitou e onde são coletados maior parte dos dados de jogo.
- Scripts relevantes para a geração: MapGenAutomata, que define como preencher o mapa, como conectar as salas, posiciona itens e inimigos; MeshGenerator, que utiliza da matriz binária 2D que representa o mapa gerado pelo script anterior para gerar a malha de colisão.


## Cenas não utilizadas:
- Main Menu: menu principal com opções, não é utilizado pois a função de salvar/carregar jogo ainda não foi implementada, então ainda não é necessário inserir a cena na hierarquia.
- PathTest: cena utilizada para testar diferentes métodos de pathfinding para os inimigos. Não é inclusa na hierarquia por não ser parte do jogo.
- Questionario: Cena que recebe input do jogador através de várias questões. Somente ao terminar o questionário que os dados são enviados para a planilha.

## Organização das pastas do projeto:
- Animations: contém pastas para separar animações dos inimigos e do jogador, aqui estão tanto as animações quanto os controles do componente "Animator" do unity.

- Imports: onde são guardadas todas as importações de assets, de modo geral, tudo que não foi criado ou alterado pelos participantes.

- Misc: objetos ainda sem classificação, no momento só existe a configuração dos gradientes de cores para texto, utilizado na UI com TextMeshPro.

- Plugins: o único plugin até então faz parte da comunicação da geração de dados com a planilha que armazena dados.

- Resources: todos os recursos que podem ser carregados via Script com funções do tipo "Resources.Load(<string>)", ou utilizando o editor visual (criar uma variável pública ou com o parâmetro "[SerializeField]" na linha anterior, e arrastando o recurso para o campo relevante do script no inspetor), por consequência, podem ser criados em tempo de execução, dado que sejam carregados no script.

- Scenes: onde devem ser salvas todas as cenas.

- Scripts: contém pastas aninhadas para subdividir os scripts em categorias, facilitando a organização. Scripts sem subpastas ou não são utilizados ou não possuem outro script da mesma categoria para necessitar de uma pasta.

- Spritesheets: como até então o jogo é, por definição, Top-Down 2D, essa é a pasta onde estão todas as imagens que compõem as animações do personagem, bem como os "ladrilhos" do mapa e inimigos. Não está junto de "Resources" por ser utilizado de forma diferente.

# Recursos fora do Unity:
- O artigo escrito durante o desenvolvimento do projeto sobre mapas procedurais pode ser encontrado em: https://drive.google.com/file/d/1IAfGnxc3TgHoshVGbXTocip9YbWa6OD8/view?usp=sharing

- Qualquer IDE capaz de compilar C#, mais comuns são Visual Studio e VS Code.

- Discord: canal de comunicação do projeto, onde dúvidas e bugs podem ser resolvidos junto aos outros integrantes.

- (Recomendado) GitHub Desktop, facilita a organização do projeto na máquina local, bem como permite commits maiores que o browser.

- (Opcional) Audacity ou similar para edição de sons, dado que raramente os sons encontrados são perfeitos para a ocasião, normalmente requerindo um ajuste.

- (Opcional) Photoshop ou similar para edição e ajuste de sprites e outras imagens.
